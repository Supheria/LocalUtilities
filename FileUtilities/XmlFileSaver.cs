using LocalUtilities.SerializeUtilities;

namespace LocalUtilities.FileUtilities;

public static class XmlFileSaver
{
    public static string? SaveToXml<T>(this XmlSerialization<T> serialization, string? path = null)
    {
        string? message = null;
        FileStream? file = null;
        try
        {

            file = File.Create(path ?? serialization.GetInitializationFilePath());
            serialization.GetXmlSerializer().Serialize(file, serialization);
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        file?.Close();
        return message;
    }
}