using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlDeserializeTool
{
    public static void ReadXmlCollection<T>(this ICollection<T> collection, XmlReader reader, string collectionName,
        XmlSerialization<T> itemSerialization)
    {
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return;
        do
        {
            if (reader.Name == collectionName && reader.NodeType is XmlNodeType.EndElement)
                return;
            if (reader.Name != itemSerialization.LocalName || reader.NodeType is not XmlNodeType.Element)
                continue;
            var item = itemSerialization.Deserialize(reader);
            collection.Add(item);
        } while (reader.Read());
        throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static void ReadXmlCollection<TKey, TValue>(this IDictionary<TKey, TValue> collection, XmlReader reader,
        string collectionName, XmlSerialization<KeyValuePair<TKey, TValue>> itemSerialization)
    {
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return;
        do
        {
            if (reader.Name == collectionName && reader.NodeType is XmlNodeType.EndElement)
                return;
            if (reader.Name != itemSerialization.LocalName || reader.NodeType is not XmlNodeType.Element)
                continue;
            var item = itemSerialization.Deserialize(reader);
            if (item.Key is not null)
                collection[item.Key] = item.Value;
        } while (reader.Read());
        throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static T Deserialize<T>(this XmlSerialization<T> serialization, XmlReader reader)
    {
        var o = serialization.GetXmlSerializer().Deserialize(reader);
        serialization = o as XmlSerialization<T> ?? serialization;
        return serialization.Source;
    }
}