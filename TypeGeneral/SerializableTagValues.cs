using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public abstract class SerializableTagValues<TKey, TValue> : ISsSerializable where TKey : notnull
{
    public Dictionary<TKey, TValue> Map { get; private set; } = [];

    protected event SerializeHandler? OnSerialize;

    protected event DeserializeHandler? OnDeserialize;

    public abstract string LocalName { get; }

    protected abstract Func<TKey, string> WriteTag { get; }

    protected abstract Func<TValue, List<string>> WriteValue { get; }

    protected abstract Func<string, TKey> ReadTag { get; }

    protected abstract Func<List<string>, TValue> ReadValue { get; }

    public TValue this[TKey key]
    {
        get => Map[key];
        set => Map[key] = value;
    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTagValuesArray("", Map, WriteTag, WriteValue);
        OnSerialize?.Invoke(serializer);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        Map = deserializer.ReadTagValuesArray("", ReadTag, ReadValue).ToDictionary();
        OnDeserialize?.Invoke(deserializer);
    }
}
