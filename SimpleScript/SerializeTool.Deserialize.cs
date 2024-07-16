using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Data.Convert;
using LocalUtilities.SimpleScript.Parser;
using System.Collections;
using System.Reflection;
using System.Text;

namespace LocalUtilities.SimpleScript;

partial class SerializeTool
{
    public static T? Deserialize<T>(byte[] buffer, int offset, int count, string? name)
    {
        var tokenizer = new Tokenizer(buffer, offset, count);
        var type = typeof(T);
        name ??= type.Name;
        if (tokenizer.Element.Property.TryGetValue(name, out var roots) || tokenizer.Element.Property.TryGetValue("", out roots))
            return (T?)Deserialize(type, roots.LastOrDefault());
        return default;
    }

    public static T? Deserialize<T>(this string str, string? name)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        return Deserialize<T>(buffer, 0, buffer.Length, name);
    }

    public static T? DeserializeFile<T>(string filePath, string? name)
    {
        var buffer = ReadFileBuffer(filePath);
        return Deserialize<T>(buffer, 0, buffer.Length, name);
    }

    private static object? Deserialize(Type type, Element? root)
    {
        if (root is null)
        {
            return null;
        }
        if (GetSimpleTypeConvert(type, out var convert))
        {
            return convert(root.Value.Text);
        }
        if (type == TPoint)
        {
            return root.Value.Text.ToPoint();
        }
        if (type == TRectangle)
        {
            return root.Value.Text.ToRectangle();
        }
        if (type == TSize)
        {
            return root.Value.Text.ToSize();
        }
        if (type == TColor)
        {
            return Color.FromName(root.Value.Text);
        }
        if (type == TDateTime)
        {
            return DateTime.FromBinary(root.Value.Text.ToLong());
        }
        if (typeof(IArrayStringConvertable).IsAssignableFrom(type))
        {
            var obj = Activator.CreateInstance(type);
            if (obj is not IArrayStringConvertable iarr)
                return null;
            iarr.ParseArrayString(root.Value.Text);
            return iarr;
        }
        if (root is not Element scope)
            return null;
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var openType = typeof(Dictionary<,>);
            var pairType = type.GetGenericArguments();
            var closeType = openType.MakeGenericType(pairType[0], pairType[1]);
            var obj = Activator.CreateInstance(closeType);
            if (!GetSimpleTypeConvert(pairType[0], out convert))
                return obj;
            foreach (var pair in scope.Property)
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
            if (scope.Property.TryGetValue("", out var items))
            {
                obj = Array.CreateInstance(itemType, items.Count);
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
            var add = type.GetMethod("Add");
            if (scope.Property.TryGetValue("", out var items))
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
            foreach (var property in type.GetProperties(Authority))
            {
                if (property.GetCustomAttribute<SsIgnore>() is not null || property.SetMethod is null)
                    continue;
                var propertyName = property.GetCustomAttribute<SsItem>()?.Name ?? property.Name;
                if (!scope.Property.TryGetValue(propertyName, out var roots))
                    continue;
                var subObj = Deserialize(property.PropertyType, roots.LastOrDefault());
                if (subObj is not null)
                    property.SetValue(obj, subObj, Authority, null, null, null);
            }
            return obj;
        }
    }
}
