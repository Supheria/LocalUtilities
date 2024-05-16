using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeBundle;

public abstract class SerializableTagValues<TKey, TValue> : ISsSerializable where TKey : notnull
{
    public Dictionary<TKey, List<TValue>> Map { get; set; } = [];

    public abstract string LocalName { get; set; }

    public abstract string KeyName { get; set; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

    protected abstract Func<string, TKey> ReadKey { get; }

    protected abstract Func<string, TValue> ReadValue { get; }

    public void Serialize(SsSerializer serializer)
    {
        foreach (var (key, value) in Map)
            serializer.WriteTagValues(KeyName, WriteKey(key), value, WriteValue);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        Map = deserializer.ReadTagValues(KeyName, ReadKey, ReadValue).ToDictionary();
    }
}
