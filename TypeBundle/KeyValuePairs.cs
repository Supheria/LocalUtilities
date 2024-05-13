using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeBundle;

public abstract class KeyValuePairs<TKey, TValue> : ISsSerializable
{
    public abstract string LocalName { get; set; }

    public List<KeyValuePair<TKey, TValue>> Pairs { get; } = [];

    protected abstract Func<string, TKey> ReadKey { get; }

    protected abstract Func<string, TValue> ReadValue { get; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

    public void Serialize(SsSerializer serializer)
    {
        foreach (var (key, value) in Pairs)
            serializer.WriteTag(WriteKey(key), WriteValue(value));
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        deserializer.Deserialize(typeof(TagValues), token =>
        {
            Pairs.Add(new(ReadKey(token.Name.Text), ReadValue(((TagValues)token).Tag.Text)));
        });
    }
}
