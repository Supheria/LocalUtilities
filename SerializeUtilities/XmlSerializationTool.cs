using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public static class XmlSerializationTool
{

    public static XmlSerializer GetXmlSerializer<T>(this XmlSerialization<T> serialization)
    {
        var XmlRoot = new XmlRootAttribute(serialization.LocalName)
        {
            Namespace = serialization.RootElementNamespace
        };
        return new XmlSerializer(serialization.GetType(), XmlRoot);
    }
}
