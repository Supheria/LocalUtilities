using LocalUtilities.SerializeUtilities;

namespace LocalUtilities.FileUtilities;

public static class XmlFileLoader
{
    public static T LoadFromXml<T>(this XmlSerialization<T> serialization, out string? message, string? path = null)
    {
        message = null;
        path ??= serialization.GetInitializationFilePath();
        if (!File.Exists(path))
            message = $"\"{path}\" file path is not existed.";
        else
        {
            FileStream? file = null;
            try
            {
                file = File.OpenRead(path);
                var o = serialization.GetXmlSerializer().Deserialize(file);
                serialization = o as XmlSerialization<T> ?? serialization;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            file?.Close();
        }
        return serialization.Source;
    }
}
