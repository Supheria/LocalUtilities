using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SerializeTool
{
    public static T ParseSsBuffer<T>(this T obj, byte[] buffer, int offset, int count) where T : ISsSerializable
    {
        var bytes = new byte[count];
        Array.Copy(buffer, offset, bytes, 0, count);
        return ParseToObject(obj, buffer);
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

    public static ICollection<T> ParseSsString<T>(this string str, string arrayName) where T : ISsSerializable, new()
    {
        try
        {
            return ParseToArray<T>(arrayName, Encoding.UTF8.GetBytes(str));
        }
        catch
        {
            return [];
        }
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
            return ParseToObject(obj, buffer);
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
            return ParseToObject(obj, buffer);
        }
        catch (Exception ex)
        {
            throw new SsParseExceptions(ex.Message);
        }
    }

    public static ICollection<T> LoadFromSimpleScript<T>(string arrayName, string filePath) where T : ISsSerializable, new()
    {
        try
        {
            var buffer = ReadFileBuffer(filePath);
            return ParseToArray<T>(arrayName, buffer);
        }
        catch (Exception ex)
        {
            throw new SsParseExceptions(ex.Message);
        }
    }

}
