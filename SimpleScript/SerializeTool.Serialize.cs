using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Serializer;
using LocalUtilities.TypeToolKit;
using LocalUtilities.TypeToolKit.Convert;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;

namespace LocalUtilities.SimpleScript;

partial class SerializeTool
{
    public static byte[] Serialize(object? obj, DataName name, SignTable signTable, Encoding? encoding)
    {
        var memory = new MemoryStream();
        using var writer = new SsStreamWriter(memory, false, signTable, encoding ?? Encoding.UTF8);
        if (Serialize(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else
            Serialize(obj, writer, name.Name, true);
        var buffer = new byte[memory.Position];
        Array.Copy(memory.GetBuffer(), 0, buffer, 0, buffer.Length);
        return buffer;
    }

    public static string Serialize(object? obj, DataName name, bool writeIntoMultiLines, SignTable signTable)
    {
        var writer = new SsStringWriter(writeIntoMultiLines, signTable);
        if (Serialize(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else
            Serialize(obj, writer, name.Name, true);
        return writer.ToString();
    }

    public static void SerializeFile(object? obj, DataName name, string filePath, bool writeIntoMultiLines, SignTable signTable)
    {
        using var file = File.Create(filePath);
        file.Write(Utf8_BOM);
        using var writer = new SsStreamWriter(file, writeIntoMultiLines, signTable, Encoding.UTF8);
        if (Serialize(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else
            Serialize(obj, writer, name.Name, true);
    }

    private static bool Serialize([NotNullWhen(false)] object? obj, [NotNullWhen(true)] out Func<object?, string>? convert)
    {
        convert = null;
        var type = obj?.GetType();
        if (obj is null)
            convert = _ => "";
        else if (type == TypeTable.Byte ||
            type == TypeTable.Char ||
            type == TypeTable.Bool ||
            type == TypeTable.Short ||
            type == TypeTable.Int ||
            type == TypeTable.Long ||
            type == TypeTable.Float ||
            type == TypeTable.Double ||
            type == TypeTable.String ||
            TypeTable.Enum.IsAssignableFrom(type))
            convert = o => o!.ToString() ?? "";
        else if (type == TypeTable.Point)
            convert = o => ((Point)o!).ToArrayString();
        else if (type == TypeTable.Rectangle)
            convert = o => ((Rectangle)o!).ToArrayString();
        else if (type == TypeTable.Size)
            convert = o => ((Size)o!).ToArrayString();
        else if (type == TypeTable.Color)
            convert = o => ((Color)o!).Name;
        else if (type == TypeTable.DateTime)
            convert = o => ((DateTime)o!).ToBinary().ToString();
        else if (typeof(IArrayStringConvertable).IsAssignableFrom(type))
            convert = o => ((IArrayStringConvertable)o!).ToArrayString();
        else
            return false;
        return true;
    }

    private static void Serialize(object? obj, SsWriter writer, string? name, bool root)
    {
        if (Serialize(obj, out var convert))
        {
            writer.AppendValue(convert(obj));
            return;
        }
        var type = obj.GetType();
        if (root)
            writer.AppendName(name ?? type.Name);
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            writer.AppendStart();
            var enumer = ((IDictionary)obj).GetEnumerator();
            for (var i = 0; i < ((IDictionary)obj).Count; i++)
            {
                enumer.MoveNext();
                if (!Serialize(enumer.Key, out convert))
                    continue;
                writer.AppendName(convert(enumer.Key));
                Serialize(enumer.Value, writer, null, false);
            }
            writer.AppendEnd();
            return;
        }
        if (typeof(ICollection).IsAssignableFrom(type))
        {
            writer.AppendStart();
            var enumer = ((ICollection)obj).GetEnumerator();
            for (var i = 0; i < ((ICollection)obj).Count; i++)
            {
                enumer.MoveNext();
                Serialize(enumer.Current, writer, null, false);
            }
            writer.AppendEnd();
            return;
        }
        else
        {
            writer.AppendStart();
            foreach (var property in type.GetProperties(Authority))
            {
                if (NotSsItem(property))
                    continue;
                var subObj = property.GetValue(obj, Authority, null, null, null);
                writer.AppendName(GetSsItemName(property));
                Serialize(subObj, writer, null, false);
            }
            writer.AppendEnd();
        }
    }
}
