using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlSerializeTool
{
    public static void WriteXmlCollection<T>(this ICollection<T> collection, XmlWriter writer, string collectionName,
        XmlSerialization<T> itemSerialization)
    {
        if (collectionName is "")
            collection.WriteXmlCollection(writer, itemSerialization);
        else if (collection.Count is 0)
            return;
        writer.WriteStartElement(collectionName);
        collection.WriteXmlCollection(writer, itemSerialization);
        writer.WriteEndElement();
    }

    public static void WriteXmlCollection<T>(this ICollection<T> collection, XmlWriter writer,
        XmlSerialization<T> itemSerialization)
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