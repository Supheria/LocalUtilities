using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.Serializations;

public abstract class KeyValuePairsSerialization<TKey, TValue> : SsSerialization<List<KeyValuePair<TKey, TValue>>>
{
    protected abstract Func<string, TKey> ReadKey { get; }

    protected abstract Func<string, TValue> ReadValue { get; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

    public KeyValuePairsSerialization() : base([])
    {
        OnSerialize += KeyValuePair_Serialize;
        OnDeserialize += KeyValuePair_Deserialize;
    }

    private void KeyValuePair_Serialize(SsSerializer serializer)
    {
        foreach (var (key, value) in Source)
            serializer.AppendTag(WriteKey(key), WriteValue(value));
    }

    private void KeyValuePair_Deserialize(Token token)
    {
        if (token is TagValues tagValues)
            Source.Add(new(ReadKey(tagValues.Name), ReadValue(tagValues.Tag)));
    }
}
