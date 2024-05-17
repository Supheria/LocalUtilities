using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SerializeTool
{
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

    public static List<T> LoadFromSimpleScript<T>(string filePath) where T : ISsSerializable, new()
    {
        try
        {
            var buffer = ReadFileBuffer(filePath);
            return ParseToArray<T>(buffer);
        }
        catch (Exception ex)
        {
            throw new SsParseExceptions(ex.Message);
        }
    }

}
