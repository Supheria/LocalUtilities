using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public static partial class SerializeTool
{
    static byte[] Utf8_BOM { get; } = [0xEF, 0xBB, 0xBF];

    private static string FormatObject<T>(T obj, bool writeIntoMultiLines) where T : ISsSerializable
    {
        var writer = new SsWriter(writeIntoMultiLines);
        var serializer = new SsSerializer(obj, writer);
        return serializer.Serialize();
    }

    private static string FormatObjects<T>(string arrayName, ICollection<T> items, bool writeIntoMultiLines) where T : ISsSerializable, new()
    {
        var writer = new SsWriter(writeIntoMultiLines);
        var serializer = new SsSerializer(new T(), writer);
        serializer.WriteObjects(arrayName, items);
        return writer.ToString();
    }

    private static void WriteUtf8File(string text, string filePath)
    {
        using var file = File.Create(filePath);
        file.Write(Utf8_BOM);
        using var streamWriter = new StreamWriter(file, Encoding.UTF8);
        streamWriter.Write(text);
        streamWriter.Close();
    }

    private static byte[] ReadFileBuffer(string filePath)
    {
        if (!File.Exists(filePath))
            throw SsParseExceptions.CannotOpenFile(filePath);
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

    private static T ParseToObject<T>(T obj, byte[] buffer) where T : ISsSerializable
    {
        var elements = new Tokenizer(buffer).Elements.Property[obj.LocalName];
        if (elements.Count is 0)
            throw SsParseExceptions.CannotFindEntry(obj.LocalName);
        if (elements.Count > 1)
            throw SsParseExceptions.MultiAssignment(obj.LocalName);
        if (elements[0] is not ElementScope scope)
            throw SsParseExceptions.WrongArrayEntry(obj.LocalName);
        var deserializer = new SsDeserializer(obj);
        deserializer.Deserialize(scope.Property);
        return obj;
    }

    private static ICollection<T> ParseToArray<T>(string arrayName, byte[] buffer) where T : ISsSerializable, new()
    {
        var list = new List<T>();
        var elements = new Tokenizer(buffer).Elements.Property[arrayName];
        if (elements.Count is 0)
            throw SsParseExceptions.CannotFindEntry(arrayName);
        if (elements.Count > 1)
            throw SsParseExceptions.MultiAssignment(arrayName);
        if (elements[0] is ElementScope scope)
        {
            var item = new T();
            new SsDeserializer(item).Deserialize(scope.Property);
            list.Add(item);
        }
        else if (elements[0] is ElementArray array)
        {
            foreach (var es in array.Properties)
            {
                var item = new T();
                new SsDeserializer(item).Deserialize(es);
                list.Add(item);
            }
        }
        else
            throw SsParseExceptions.WrongArrayEntry(arrayName);
        return list;
    }
}
