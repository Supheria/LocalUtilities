using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class KeyValuePairXmlSerialization<TKey, TValue>() : XmlSerialization<KeyValuePair<TKey, TValue>>(new())
{
    protected abstract string KeyName { get; }

    protected abstract string ValueName { get; }

    protected abstract Func<string?, TKey> ReadKey { get; }

    protected abstract Func<string?, TValue> ReadValue { get; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

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
