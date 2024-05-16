using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using System;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SerializeTool
{
    public static byte[] Utf8_BOM { get; } = [0xEF, 0xBB, 0xBF];

    public static void SaveToSimpleScript<T>(this T obj, bool writeIntoMultiLines, string? outFilePath = null) where T : ISsSerializable
    {
        var writer = new SsWriter(writeIntoMultiLines);
        var serializer = new SsSerializer(obj, writer);
        var path = outFilePath ?? serializer.GetInitializationFilePath();
        using var file = File.Create(path);
        file.Write(Utf8_BOM);
        using var streamWriter = new StreamWriter(file, Encoding.UTF8);
        streamWriter.Write(serializer.Serialize());
        streamWriter.Close();
    }

    public static T ParseSsString<T>(this T obj, string str) where T : ISsSerializable
    {
        try
        {
            return ParseToObject(obj, Encoding.UTF8.GetBytes(str));
        }
        catch
        {
            return obj;
        }
    }

    public static List<T> ParseSsString<T>(this string str) where T : ISsSerializable, new()
    {
        try
        {
            return ParseToArray<T>(Encoding.UTF8.GetBytes(str));
        }
        catch
        {
            return [];
        }
    }

    public static T LoadFromSimpleScript<T>(this T obj) where T : ISsSerializable
    {
        try
        {
            var deserializer = new SsDeserializer(obj);
            var buffer = ReadFileBuffer(deserializer.GetInitializationFilePath());
            return ParseToObject(obj, buffer);
        }
        catch
        {
            SaveToSimpleScript(obj, true);
            return obj;
        }
    }

    public static T LoadFromSimpleScript<T>(this T obj, string filePath) where T : ISsSerializable
    {
        var buffer = ReadFileBuffer(filePath);
        return ParseToObject(obj, buffer);
    }

    public static List<T> LoadFromSimpleScript<T>(string filePath) where T: ISsSerializable, new()
    {
        var buffer = ReadFileBuffer(filePath);
        return ParseToArray<T>(buffer);
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

    private static T ParseToObject<T>(this T obj, byte[] buffer) where T : ISsSerializable
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

    private static List<T> ParseToArray<T>(byte[] buffer) where T : ISsSerializable, new()
    {
        var list = new List<T>();
        var item = new T();
        var elements = new Tokenizer(buffer).Elements.Property[item.LocalName];
        if (elements.Count is 0)
            throw SsParseExceptions.CannotFindEntry(item.LocalName);
        if (elements.Count > 1)
            throw SsParseExceptions.MultiAssignment(item.LocalName);
        if (elements[0] is ElementScope scope)
        {
            new SsDeserializer(item).Deserialize(scope.Property);
            list.Add(item);
        }
        else if (elements[0] is ElementArray array)
        {
            foreach (var es in array.Properties)
            {
                item = new T();
                new SsDeserializer(item).Deserialize(es);
                list.Add(item);
            }
        }
        else
            throw SsParseExceptions.WrongArrayEntry(item.LocalName);
        return list;
    }
}
