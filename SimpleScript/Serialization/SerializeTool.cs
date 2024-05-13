using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Parser;
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
        streamWriter.Write(serializer.ToString());
        streamWriter.Close();
    }

    public static T LoadFromSimpleScript<T>(this T sample, string? inFilePath = null) where T : ISsSerializable
    {
        var deserializer = new SsDeserializer(sample);
        var path = inFilePath ?? deserializer.GetInitializationFilePath();
        if (!File.Exists(path))
            throw new SsParseExceptions($"could not open file: {path}");
        byte[] buffer;
        using var file = File.OpenRead(path);
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
        return successful ? (T)deserializer.Source : throw new SsParseExceptions($"cannot find an entry of {sample.LocalName}");
    }
}
