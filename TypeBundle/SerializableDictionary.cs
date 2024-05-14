using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeBundle;

public abstract class SerializableDictionary<TKey, TValue> : ISsSerializable where TKey : notnull
{
    public abstract string LocalName { get; set; }

    public Dictionary<TKey, TValue> Map { get; protected set; } = [];

    protected abstract Func<string, TKey> ReadKey { get; }

    protected abstract Func<string, TValue> ReadValue { get; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

    public void Serialize(SsSerializer serializer)
    {
        foreach (var (key, value) in Map)
            serializer.WriteTag(WriteKey(key), WriteValue(value));
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        Map.Clear();
        deserializer.Deserialize<TagValues>(tagValues =>
        {
            Map[ReadKey(tagValues.Name.Text)] = ReadValue(tagValues.Tag.Text);
        });
    }
}
