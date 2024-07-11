using LocalUtilities.SimpleScript.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeGeneral;

public abstract class SerializableTagValues<TKey, TValue> : ISsSerializable, IDictionary<TKey, TValue> where TKey : notnull
{
    Dictionary<TKey, TValue> Map { get; set; } = [];

    protected event SerializeHandler? OnSerialize;

    protected event DeserializeHandler? OnDeserialize;

    public abstract string LocalName { get; }

    protected abstract Func<TKey, string> WriteTag { get; }

    protected abstract Func<TValue, List<string>> WriteValue { get; }

    protected abstract Func<string, TKey> ReadTag { get; }

    protected abstract Func<List<string>, TValue> ReadValue { get; }

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

    public ICollection<TKey> Keys => Map.Keys;

    public ICollection<TValue> Values => Map.Values;

    public int Count => Map.Count;

    public bool IsReadOnly => false;

    public TValue this[TKey key]
    {
        get => Map[key];
        set => Map[key] = value;
    }

    public void Add(TKey key, TValue value)
    {
        Map.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return Map.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return Map.Remove(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Map.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Map.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return Map.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        Map.ToList().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Map.Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return Map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Map.GetEnumerator();
    }
}
