using LocalUtilities.SerializeUtilities;
using System.Xml.Serialization;

namespace LocalUtilities.ManageUtilities;

public static class XmlFileSaverLoader
{
    public static void SaveToXml<T>(this T obj, string path, XmlSerialization<T> serialization)
    {
        serialization.Source = obj;
        var file = File.Create(path);
        var writer = new XmlSerializer(serialization.GetType());
        writer.Serialize(file, serialization);
        file.Close();
    }

    public static string LoadFromXml<T>(this XmlSerialization<T> serialization, string path, out T? obj)
    {
        obj = default;
        if (!File.Exists(path))
            return $"{path} is not existed.";
        var file = File.OpenRead(path);
        try
        {
            var reader = new XmlSerializer(serialization.GetType());
            var o = reader.Deserialize(file);
            serialization = o as XmlSerialization<T> ?? serialization;
            obj = serialization.Source;
            file.Close();
            return "";
        }
        catch (Exception e)
        {
            file.Close();
            return e.Message;
        }
    }

    public static T? LoadFromXml<T>(this XmlSerialization<T> serialization, string path)
    {
        if (!File.Exists(path))
            return serialization.Source;
        var file = File.OpenRead(path);
        try
        {
            var reader = new XmlSerializer(serialization.GetType());
            var o = reader.Deserialize(file);
            serialization = o as XmlSerialization<T> ?? serialization;
            file.Close();
        }
        catch
        {
            file.Close();
        }
        return serialization.Source;
    }
}