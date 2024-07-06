using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SerializeTool
{
    public static byte[] ToSsBuffer<T>(this T obj) where T : ISsSerializable
    {
        var str = FormatObject(obj, false);
        return Encoding.UTF8.GetBytes(str);
    }

    public static string ToSsString<T>(this T obj) where T : ISsSerializable
    {
        return FormatObject(obj, false);
    }

    public static string ToSsString<T>(this ICollection<T> items, string arrayName) where T : ISsSerializable, new()
    {
        try
        {
            return FormatObjects(arrayName, items, false);
        }
        catch
        {
            return "";
        }
    }

    public static void SaveToSimpleScript<T>(this T obj, bool writeIntoMultiLines) where T : ISsSerializable
    {
        try
        {
            var deserializer = new SsSerializer(obj, new(writeIntoMultiLines));
            var text = FormatObject(obj, writeIntoMultiLines);
            WriteUtf8File(text, deserializer.GetInitializeFilePath());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"cannot save to ini-file: {ex.Message}");
        }
    }

    public static void SaveToSimpleScript<T>(this T obj, bool writeIntoMultiLines, string filePath) where T : ISsSerializable
    {
        try
        {
            var text = FormatObject(obj, writeIntoMultiLines);
            WriteUtf8File(text, filePath);
        }
        catch (Exception ex)
        {
            throw new SsFormatException(ex.Message);
        }
    }

    public static void SaveToSimpleScript<T>(this ICollection<T> items, string arrayName, bool writeIntoMultiLines, string filePath) where T : ISsSerializable, new()
    {
        try
        {
            var text = FormatObjects(arrayName, items, writeIntoMultiLines);
            WriteUtf8File(text, filePath);
        }
        catch (Exception ex)
        {
            throw new SsFormatException(ex.Message);
        }
    }
}
