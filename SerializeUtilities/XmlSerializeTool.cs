using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlSerializeTool
{
    public static void WriteXmlCollection<T>(this XmlSerialization<T> itemSerialization, ICollection<T> collection, XmlWriter writer, string collectionName)
    {
        writer.WriteStartElement(collectionName);
        foreach (var item in collection)
        {
            itemSerialization.Source = item;
            itemSerialization.Serialize(writer);
        }
        writer.WriteEndElement();
    }

   

    public static void Serialize<T>(this XmlSerialization<T> serialization, XmlWriter writer)
    {
        serialization.GetXmlSerializer().Serialize(writer, serialization);
    }
}