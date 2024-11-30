using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Serializer;
using LocalUtilities.TypeToolKit;
using LocalUtilities.TypeToolKit.Convert;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LocalUtilities.SimpleScript;

partial class SerializeTool
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="signTable"></param>
    /// <param name="encoding">set null to use default value of <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    public static byte[] Serialize(object? obj, RootName name, SignTable signTable, Encoding? encoding)
    {
        using var memory = new MemoryStream();
        using var writer = new SsStreamWriter(memory, false, signTable, encoding ?? Encoding.UTF8);
        if (SerializeSimpleType(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else if (name.Name is null)
            Serialize(obj, writer, false);
        else
        {
            writer.AppendName(name.Name);
            Serialize(obj, writer, true);
        }
        var buffer = new byte[memory.Position];
        Array.Copy(memory.GetBuffer(), 0, buffer, 0, buffer.Length);
        return buffer;
    }

    public static string Serialize(object? obj, RootName name, SignTable signTable, bool writeIntoMultiLines)
    {
        var writer = new SsStringWriter(writeIntoMultiLines, signTable);
        if (SerializeSimpleType(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else if (name.Name is null)
            Serialize(obj, writer, false);
        else
        {
            writer.AppendName(name.Name);
            Serialize(obj, writer, true);
        }
        return writer.ToString();
    }

    public static void SerializeFile(object? obj, RootName name, SignTable signTable, bool writeIntoMultiLines, string filePath)
    {
        using var file = File.Create(filePath);
        file.Write(Utf8_BOM);
        using var writer = new SsStreamWriter(file, writeIntoMultiLines, signTable, Encoding.UTF8);
        if (SerializeSimpleType(obj, out var convert))
            writer.AppendUnquotedValue(convert(obj));
        else if (name.Name is null)
            Serialize(obj, writer, false);
        else
        {
            writer.AppendName(name.Name);
            Serialize(obj, writer, true);
        }
    }

    private static bool SerializeSimpleType([NotNullWhen(false)] object? obj, [NotNullWhen(true)] out Func<object?, string>? convert)
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
        else if (type == TypeTable.DateTime)
            convert = o => ((DateTime)o!).ToBinary().ToString();
        else
            return false;
        return true;
    }

    private static void Serialize(object? obj, SsWriter writer, bool appendBrace)
    {
        if (SerializeSimpleType(obj, out var convert))
        {
            writer.AppendValue(convert(obj));
            return;
        }
        var type = obj.GetType();
        if (appendBrace)
            writer.AppendStart();
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            var enumer = ((IDictionary)obj).GetEnumerator();
            for (var i = 0; i < ((IDictionary)obj).Count; i++)
            {
                enumer.MoveNext();
                if (!SerializeSimpleType(enumer.Key, out convert))
                    continue;
                writer.AppendName(convert(enumer.Key));
                Serialize(enumer.Value, writer, true);
            }
        }
        else if (typeof(ICollection).IsAssignableFrom(type))
        {
            var enumer = ((ICollection)obj).GetEnumerator();
            for (var i = 0; i < ((ICollection)obj).Count; i++)
            {
                enumer.MoveNext();
                Serialize(enumer.Current, writer, true);
            }
        }
        else
        {
            foreach (var property in type.GetProperties(Authority))
            {
                if (NotSsItem(property))
                    continue;
                var subObj = property.GetValue(obj, Authority, null, null, null);
                writer.AppendName(GetSsItemName(property));
                Serialize(subObj, writer, true);
            }
        }
        if (appendBrace)
            writer.AppendEnd();
    }
}
