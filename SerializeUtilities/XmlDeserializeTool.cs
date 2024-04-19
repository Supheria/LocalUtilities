using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public static class XmlDeserializeTool
{
    public static T[] ReadXmlCollection<T>(this XmlSerialization<T> itemSerialization, XmlReader reader, string collectionName)
    {
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return [];
        var collection = new List<T>();
        do
        {
            if (reader.Name == collectionName && reader.NodeType is XmlNodeType.EndElement)
                break;
            if (reader.Name != itemSerialization.LocalName || reader.NodeType is not XmlNodeType.Element)
                continue;
            collection.Add(itemSerialization.Deserialize(reader));
        } while (reader.Read());
        return collection.ToArray();
        //throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static Dictionary<TKey, TValue> ReadXmlCollection<TKey, TValue>(
        this XmlSerialization<KeyValuePair<TKey, TValue>> itemSerialization, XmlReader reader, string collectionName
        ) where TKey : notnull
    {
        var collection = new Dictionary<TKey, TValue>();
        // 子节点探针
        if (reader.ReadToDescendant(itemSerialization.LocalName) is false)
            return collection;
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
        return collection;
        //throw new($"读取 {itemSerialization.LocalName} 时未能找到结束标签");
    }

    public static T Deserialize<T>(this XmlSerialization<T> serialization, XmlReader reader)
    {
        var o = serialization.GetXmlSerializer().Deserialize(reader);
        serialization = o as XmlSerialization<T> ?? serialization;
        return serialization.Source;
    }
}