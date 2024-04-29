using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlDeserializeTool
{
    public static void ReadXmlCollection<T>(this ICollection<T> collection, XmlReader reader, XmlSerialization<T> itemSerialization, string collectionName)
    {
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return;
        do
        {
            if (reader.Name == collectionName && reader.NodeType is XmlNodeType.EndElement)
                break;
            if (reader.Name != itemSerialization.LocalName || reader.NodeType is not XmlNodeType.Element)
                continue;
            collection.Add(itemSerialization.Deserialize(reader));
        } while (reader.Read());
        //throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static void ReadXmlCollection<TKey, TValue>(this Dictionary<TKey, TValue> collection, XmlReader reader, XmlSerialization<KeyValuePair<TKey, TValue>> itemSerialization, string collectionName)
        where TKey : notnull
    {
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return;
        do
        {
            if (reader.Name == collectionName && reader.NodeType is XmlNodeType.EndElement)
                break;
            if (reader.Name != itemSerialization.LocalName || reader.NodeType is not XmlNodeType.Element)
                continue;
            var item = itemSerialization.Deserialize(reader);
            if (item.Key is not null)
                collection[item.Key] = item.Value;
        } while (reader.Read());
        return;
        //throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static T Deserialize<T>(this XmlSerialization<T> serialization, XmlReader reader)
    {
        var o = serialization.GetXmlSerializer().Deserialize(reader);
        serialization = o as XmlSerialization<T> ?? serialization;
        return serialization.Source;
    }
}