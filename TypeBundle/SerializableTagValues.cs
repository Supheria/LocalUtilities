using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeBundle;

public abstract class SerializableTagValues<TKey, TValue> : ISsSerializable where TKey : notnull
{
    public Dictionary<TKey, TValue> Map { get; set; } = [];

    public abstract string LocalName { get; set; }

    protected abstract Func<TKey, string> WriteTag { get; }

    protected abstract Func<TValue, List<string>> WriteValue { get; }

    protected abstract Func<string, TKey> ReadTag { get; }

    protected abstract Func<List<string>, TValue> ReadValue { get; }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTagValuesArray("", Map, WriteTag, WriteValue);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        Map = deserializer.ReadTagValuesArray("", ReadTag, ReadValue).ToDictionary();
    }
}
