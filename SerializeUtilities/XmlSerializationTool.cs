using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public static class XmlSerializationTool
{
    public static Dictionary<(Type, string), XmlSerializer> XmlSerializerCache { get; } = [];

    public static XmlSerializer GetXmlSerializer<T>(this XmlSerialization<T> serialization)
    {
        var signature = (serialization.GetType(), serialization.LocalName);
        if (XmlSerializerCache.TryGetValue(signature, out var serializer))
            return serializer;
        var XmlRoot = new XmlRootAttribute(serialization.LocalName) {
            Namespace = serialization.RootElementNamespace
        };
        serializer = new XmlSerializer(serialization.GetType(), XmlRoot);
        XmlSerializerCache.Add(signature, serializer);
        return serializer;
    }
}
