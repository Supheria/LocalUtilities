using LocalUtilities.FileUtilities;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SsSerializeTool
{
    public static string? SaveToFile<T>(this SsSerialization<T> serialization)
    {
        string? message = null;
        FileStream? file = null;
        try
        {
            file = File.Create(serialization.GetInitializationFilePath());
            var writer = new SsSerializer();
            serialization.Serializ(writer);
            file.Write([0xEF, 0xBB, 0xBF]);
            file.Write(writer.GetBuffer());
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        file?.Close();
        file?.Dispose();
        return message;
    }

    public static void Serialize<T>(this ICollection<T> collection, SsSerializer writer, SsSerialization<T> itemSerialization)
    {
        foreach (var item in collection)
        {
            itemSerialization.Source = item;
            itemSerialization.Serialize(writer);
        }
    }

    public static void Serialize<T>(this SsSerialization<T> serialization, SsSerializer writer)
    {
        serialization.Serializ(writer);
    }
}
