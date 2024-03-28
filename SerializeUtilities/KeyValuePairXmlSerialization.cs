using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public class KeyValuePairXmlSerialization<TKey, TValue> : XmlSerialization<KeyValuePair<TKey, TValue>>
{
    string _keyName;
    string _valueName;
    Func<string?, TKey> _readKey;
    Func<string?, TValue> _readValue;
    Func<TKey, string> _writeKey;
    Func<TValue, string> _writeValue;

    public KeyValuePairXmlSerialization(string localRootName, string keyName, string valueName,
        Func<string?, TKey> readKey, Func<string?, TValue> readValue,
        Func<TKey, string> writeKey, Func<TValue, string> writeValue)
        : base(localRootName)
    {
        _keyName = keyName;
        _valueName = valueName;
        _readKey = readKey;
        _readValue = readValue;
        _writeKey = writeKey;
        _writeValue = writeValue;
    }

    public override void ReadXml(XmlReader reader) =>
        Source = new(_readKey(reader.GetAttribute(_keyName)), _readValue(reader.GetAttribute(_valueName)));

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString(_keyName, _writeKey(Source.Key));
        writer.WriteAttributeString(_valueName, _writeValue(Source.Value));
    }
}
