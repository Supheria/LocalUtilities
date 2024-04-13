using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class KeyValuePairXmlSerialization<TKey, TValue>(
    string localName, string keyName, string valueName,
    Func<string?, TKey> readKey, Func<string?, TValue> readValue,
    Func<TKey, string> writeKey, Func<TValue, string> writeValue)
    : XmlSerialization<KeyValuePair<TKey, TValue>>(new())
{
    string KeyName => keyName;

    string ValueName => valueName;

    Func<string?, TKey> ReadKey => readKey;

    Func<string?, TValue> ReadValue => readValue;

    Func<TKey, string> WriteKey => writeKey;

    Func<TValue, string> WriteValue => writeValue;

    public override string LocalName => localName;

    public override void ReadXml(XmlReader reader)
    {
        Source = new(ReadKey(reader.GetAttribute(KeyName)), ReadValue(reader.GetAttribute(ValueName)));
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString(KeyName, WriteKey(Source.Key));
        writer.WriteAttributeString(ValueName, WriteValue(Source.Value));
    }
}
