using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlSerializeTool
{
    public static void WriteXmlCollection<T>(this ICollection<T> collection, XmlWriter writer, XmlSerialization<T> itemSerialization, string collectionName)
    {
        writer.WriteStartElement(collectionName); 
        WriteXmlCollection(collection, writer, itemSerialization);
        writer.WriteEndElement();
    }

    public static void WriteXmlCollection<T>(this ICollection<T> collection, XmlWriter writer, XmlSerialization<T> itemSerialization)
    {
        foreach (var item in collection)
        {
            itemSerialization.Source = item;
            itemSerialization.Serialize(writer);
        }
    }

    public static void Serialize<T>(this XmlSerialization<T> serialization, XmlWriter writer)
    {
        serialization.GetXmlSerializer().Serialize(writer, serialization);
    }
}