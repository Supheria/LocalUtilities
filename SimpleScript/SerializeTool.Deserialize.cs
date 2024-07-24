using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeToolKit;
using LocalUtilities.TypeToolKit.Convert;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LocalUtilities.SimpleScript;

partial class SerializeTool
{
    public static object? Deserialize(Type type, DataName name, byte[] buffer, int offset, int count, SignTable signTable, Encoding? encoding)
    {
        encoding ??= Encoding.UTF8;
        if (Deserialize(type, out var convert))
        {
            var str = encoding.GetString(buffer, offset, count);
            return convert(str);
        }
        var tokenizer = new Tokenizer(buffer, offset, count, signTable, encoding);
        if (tokenizer.Element.Property.TryGetValue(name.Name, out var roots) || tokenizer.Element.Property.TryGetValue("", out roots))
            return Deserialize(type, roots.LastOrDefault());
        return null;
    }

    public static object? Deserialize(Type type, DataName name, string str, SignTable signTable)
    {
        if (Deserialize(type, out var convert))
            return convert(str);
        var tokenizer = new Tokenizer(str, signTable);
        if (tokenizer.Element.Property.TryGetValue(name.Name, out var roots) || tokenizer.Element.Property.TryGetValue("", out roots))
            return Deserialize(type, roots.LastOrDefault());
        return null;
    }

    private static bool Deserialize(Type type, [NotNullWhen(true)] out Func<string, object?>? convert)
    {
        convert = null;
        if (type == TypeTable.Byte)
            convert = str => str.ToByte();
        else if (type == TypeTable.Char)
            convert = str => str.ToChar();
        else if (type == TypeTable.Bool)
            convert = str => str.ToBool();
        else if (type == TypeTable.Short)
            convert = str => str.ToShort();
        else if (type == TypeTable.Int)
            convert = str => str.ToInt();
        else if (type == TypeTable.Long)
            convert = str => str.ToLong();
        else if (type == TypeTable.Float)
            convert = str => str.ToFloat();
        else if (type == TypeTable.Double)
            convert = str => str.ToDouble();
        else if (type == TypeTable.String)
            convert = str => str;
        else if (TypeTable.Enum.IsAssignableFrom(type))
            convert = str => str.ToEnum(type);
        else if (type == TypeTable.Point)
            convert = str => str.ToPoint();
        else if (type == TypeTable.Rectangle)
            convert = str => str.ToRectangle();
        else if (type == TypeTable.Size)
            convert = str => str.ToSize();
        else if (type == TypeTable.Color)
            convert = str => Color.FromName(str);
        else if (type == TypeTable.DateTime)
            convert = str => DateTime.FromBinary(str.ToLong());
        else if (typeof(IArrayStringConvertable).IsAssignableFrom(type))
        {
            convert = str =>
            {
                var obj = Activator.CreateInstance(type);
                if (obj is not IArrayStringConvertable iarr)
                    return null;
                iarr.ParseArrayString(str);
                return iarr;
            };
        }
        else
            return false;
        return true;
    }

    private static object? Deserialize(Type type, Element? root)
    {
        if (root is null)
            return null;
        if (Deserialize(type, out var convert))
            return convert(root.Value.Text);
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var openType = typeof(Dictionary<,>);
            var pairType = type.GetGenericArguments();
            var closeType = openType.MakeGenericType(pairType[0], pairType[1]);
            var obj = Activator.CreateInstance(closeType);
            if (obj is null)
                return null;
            if (!Deserialize(pairType[0], out convert))
                return obj;
            foreach (var pair in root.Property)
            {
                var key = convert(pair.Key);
                var value = Deserialize(pairType[1], pair.Value.FirstOrDefault());
                var add = type.GetMethod("Add");
                if (value is not null)
                    add?.Invoke(obj, [key, value]);
            }
            return obj;
        }
        if (typeof(ICollection).IsAssignableFrom(type) && type.IsArray)
        {
            var itemType = type.GetElementType()!;
            object? obj = null;
            if (root.Property.TryGetValue("", out var items))
            {
                obj = Array.CreateInstance(itemType, items.Count);
                if (obj is null)
                    return null;
                for (var i = 0; i < items.Count; i++)
                {
                    var itemObj = Deserialize(itemType, items[i]);
                    if (itemObj is not null)
                        ((Array)obj).SetValue(itemObj, i);
                }
            }
            return obj;
        }
        else if (typeof(ICollection).IsAssignableFrom(type))
        {
            var itemType = type.GetGenericArguments()[0];
            var openType = typeof(List<>);
            var closeType = openType.MakeGenericType(itemType);
            var obj = Activator.CreateInstance(closeType);
            if (obj is null)
                return null;
            var add = type.GetMethod("Add");
            if (root.Property.TryGetValue("", out var items))
            {
                foreach (var item in items)
                {
                    var itemObj = Deserialize(itemType, item);
                    if (itemObj is not null)
                        add?.Invoke(obj, [itemObj]);
                }
            }
            return obj;
        }
        else
        {
            var obj = Activator.CreateInstance(type);
            if (obj is null)
                return null;
            foreach (var property in type.GetProperties(Authority))
            {
                if (NotSsItem(property))
                    continue;
                if (!root.Property.TryGetValue(GetSsItemName(property), out var roots))
                    continue;
                var subObj = Deserialize(property.PropertyType, roots.LastOrDefault());
                if (subObj is not null)
                    property.SetValue(obj, subObj, Authority, null, null, null);
            }
            return obj;
        }
    }

    public static T? Deserialize<T>(DataName name, byte[] buffer, int offset, int count, SignTable signTable, Encoding? encoding)
    {
        return (T?)Deserialize(typeof(T), name, buffer, offset, count, signTable, encoding);
    }

    public static T? Deserialize<T>(DataName name, string str, SignTable signTable)
    {
        return (T?)Deserialize(typeof(T), name, str, signTable);
    }

    public static T? DeserializeFile<T>(DataName name, SignTable signTable, string filePath)
    {
        var buffer = ReadFileBuffer(filePath);
        return (T?)Deserialize(typeof(T), name, buffer, 0, buffer.Length, signTable, Encoding.UTF8);
    }

    public static object? DeserializeFile(Type type, DataName name, SignTable signTable, string filePath)
    {
        var buffer = ReadFileBuffer(filePath);
        return Deserialize(type, name, buffer, 0, buffer.Length, signTable, Encoding.UTF8);
    }

    private static byte[] ReadFileBuffer(string filePath)
    {
        if (!File.Exists(filePath))
            throw SsParseException.CannotOpenFile(filePath);
        byte[] buffer;
        using var file = File.OpenRead(filePath);
        if (file.ReadByte() == Utf8_BOM[0] && file.ReadByte() == Utf8_BOM[1] && file.ReadByte() == Utf8_BOM[2])
        {
            buffer = new byte[file.Length - 3];
            _ = file.Read(buffer, 0, buffer.Length);
        }
        else
        {
            file.Seek(0, SeekOrigin.Begin);
            buffer = new byte[file.Length];
            _ = file.Read(buffer, 0, buffer.Length);
        }
        return buffer;
    }
}
