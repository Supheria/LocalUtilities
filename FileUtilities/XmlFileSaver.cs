using LocalUtilities.SerializeUtilities;

namespace LocalUtilities.FileUtilities;

public static class XmlFileSaver
{
    public static void SaveToXml<T>(this XmlSerialization<T> serialization, string? path = null)
    {
        var file = File.Create(path ?? serialization.GetInitializationFilePath());
        serialization.GetXmlSerializer().Serialize(file, serialization);
        file.Close();
    }
}