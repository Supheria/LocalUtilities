using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SerializeTool
{
    public static T ParseSs<T>(this T obj, byte[] buffer, int offset, int count) where T : ISsSerializable
    {
        return ParseToObject(obj, buffer, offset, count);
    }

    public static T ParseSs<T>(this T obj, string str) where T : ISsSerializable
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        return ParseToObject(obj, buffer, 0, buffer.Length);
    }

    public static List<T> ParseSs<T>(string arrayName, byte[] buffer, int offset, int count) where T : ISsSerializable, new()
    {
        return ParseToArray<T>(arrayName, buffer, offset, count);
    }

    public static List<T> ParseSs<T>(string arrayName, string str) where T : ISsSerializable, new()
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        return ParseToArray<T>(arrayName, buffer, 0, buffer.Length);
    }

    /// <summary>
    /// load ini-file with default name of <see cref="ISsSerializable.LocalName"/>, loading failure will write <paramref name="obj"/> as default value into ini-file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T LoadFromSimpleScript<T>(this T obj) where T : ISsSerializable
    {
        try
        {
            var deserializer = new SsDeserializer(obj);
            var buffer = ReadFileBuffer(deserializer.GetInitializeFilePath());
            return ParseToObject(obj, buffer, 0, buffer.Length);
        }
        catch
        {
            SaveToSimpleScript(obj, true);
            return obj;
        }
    }

    /// <summary>
    /// load file from given path, loading failure will cause <see cref="SsParseExceptions"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions"></exception>
    public static T LoadFromSimpleScript<T>(this T obj, string filePath) where T : ISsSerializable
    {
        try
        {
            var buffer = ReadFileBuffer(filePath);
            return ParseToObject(obj, buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            throw new SsParseExceptions(ex.Message);
        }
    }

    public static List<T> LoadFromSimpleScript<T>(string arrayName, string filePath) where T : ISsSerializable, new()
    {
        try
        {
            var buffer = ReadFileBuffer(filePath);
            return ParseToArray<T>(arrayName, buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            throw new SsParseExceptions(ex.Message);
        }
    }
}
