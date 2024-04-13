using LocalUtilities.SerializeUtilities;

namespace LocalUtilities.FileUtilities;

public static class XmlFileLoader
{
    public static T LoadFromXml<T>(this XmlSerialization<T> serialization, out string message, string? path = null)
    {
        message = "";
        path ??= serialization.GetInitializationFilePath();
        if (!File.Exists(path))
            message = $"{path} is not existed.";
        else
        {
            var file = File.OpenRead(path);
            try
            {
                var o = serialization.GetXmlSerializer().Deserialize(file);
                serialization = o as XmlSerialization<T> ?? serialization;
                file.Close();
            }
            catch (Exception e)
            {
                file.Close();
                message = e.Message;
            }
        }
        return serialization.Source;
    }
}
