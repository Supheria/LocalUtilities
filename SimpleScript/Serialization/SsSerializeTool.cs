using LocalUtilities.FileUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SsSerializeTool
{
    public static string? SaveToFile<T>(this SsSerialization<T> serialization, bool writeIntoMultiLines)
    {
        string? message = null;
        try
        {
            using var file = File.Create(serialization.GetInitializationFilePath());
            var serializer = new SsSerializer(writeIntoMultiLines);
            serialization.Serialize(serializer);
            file.Write([0xEF, 0xBB, 0xBF]);
            using var streamWriter = new StreamWriter(file, Encoding.UTF8);
            streamWriter.Write(serializer.ToString());
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return message;
    }
}
