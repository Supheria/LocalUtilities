using LocalUtilities.SimpleScript.Data.Convert;
using LocalUtilities.SimpleScript.Serializer;
using System.Collections;
using System.Reflection;
using System.Text;

namespace LocalUtilities.SimpleScript;

partial class SerializeTool
{
    public static string Serialize(this object obj, bool writeIntoMultiLines, string? name)
    {
        var writer = new SsWriter(writeIntoMultiLines);
        Serialize(obj, writer, name, true);
        return writer.ToString();
    }

    public static byte[] Serialize(this object obj, string? name)
    {
        var str = Serialize(obj, false, name);
        return Encoding.UTF8.GetBytes(str);
    }

    public static void SerializeFile(this object obj, bool writeIntoMultiLines, string filePath, string? name)
    {
        var text = Serialize(obj, writeIntoMultiLines, name);
        WriteUtf8File(text, filePath);
    }

    private static void Serialize(object? obj, SsWriter writer, string? name, bool root)
    {
        if (obj is null)
        {
            writer.AppendValue("");
            return;
        }
        var type = obj.GetType();
        if (IsSimpleType(type))
        {
            writer.AppendValue(obj.ToString() ?? "");
            return;
        }
        if (type == TPoint)
        {
            writer.AppendValue(((Point)obj).ToArrayString());
            return;
        }
        if (type == TRectangle)
        {
            writer.AppendValue(((Rectangle)obj).ToArrayString());
            return;
        }
        if (type == TSize)
        {
            writer.AppendValue(((Size)obj).ToArrayString());
            return;
        }
        if (type == TColor)
        {
            writer.AppendValue(((Color)obj).Name);
            return;
        }
        if (type == TDateTime)
        {
            writer.AppendValue(((DateTime)obj).ToBinary().ToString());
            return;
        }
        if (typeof(IArrayStringConvertable).IsAssignableFrom(type))
        {
            writer.AppendValue(((IArrayStringConvertable)obj).ToArrayString());
            return;
        }
        if (root)
            writer.AppendName(name ?? type.Name);
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var pairType = type.GetGenericArguments();
            if (!IsSimpleType(pairType[0]))
            {
                writer.AppendValue("");
                return;
            }
            writer.AppendStart();
            var enumer = ((IDictionary)obj).GetEnumerator();
            for (var i = 0; i < ((IDictionary)obj).Count; i++)
            {
                enumer.MoveNext();
                if (enumer.Value is null)
                    continue;
                writer.AppendName(enumer.Key.ToString() ?? "");
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
                if (property.GetCustomAttribute<SsIgnore>() is not null || property.SetMethod is null)
                    continue;
                var subObj = property.GetValue(obj, Authority, null, null, null);
                var propertyName = property.GetCustomAttribute<SsItem>()?.Name ?? property.Name;
                writer.AppendName(propertyName);
                Serialize(subObj, writer, null, false);
            }
            writer.AppendEnd();
        }
    }
}
