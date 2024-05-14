using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Parser;
using System;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SerializeTool
{
    public static byte[] Utf8_BOM { get; } = [0xEF, 0xBB, 0xBF];

    public static T ParseSsString<T>(this T obj, string str) where T : ISsSerializable
    {
        var deserializer = new SsDeserializer(obj);
        var successful = false;
        foreach (var token in new Tokenizer(str).Tokens)
        {
            if (deserializer.Deserialize(token))
            {
                successful = true;
                break;
            }
        }
        return successful ? obj : throw SsParseExceptions.CannotFindEntry(obj.LocalName);
    }

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

    public static T LoadFromSimpleScript<T>(this T obj) where T : ISsSerializable
    {
        try
        {
            var deserializer = new SsDeserializer(obj);
            return LoadFromSimpleScript(obj, deserializer.GetInitializationFilePath());
        }
        catch
        {
            SaveToSimpleScript(obj, true);
            return obj;
        }
    }

    public static T LoadFromSimpleScript<T>(this T obj, string inFilePath) where T : ISsSerializable
    {
        try
        {
            return load(obj, inFilePath);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        static T load(T obj, string inFilePath)
        {
            var deserializer = new SsDeserializer(obj);
            if (!File.Exists(inFilePath))
                throw SsParseExceptions.CannotOpenFile(inFilePath);
            byte[] buffer;
            using var file = File.OpenRead(inFilePath);
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
            var successful = false;
            foreach (var token in new Tokenizer(buffer).Tokens)
            {
                if (deserializer.Deserialize(token))
                {
                    successful = true;
                    break;
                }
            }
            return successful ? obj : throw SsParseExceptions.CannotFindEntry(obj.LocalName, inFilePath);
        }
    }
}
