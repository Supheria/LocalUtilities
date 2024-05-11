using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.Serializations;

public abstract class KeyValuePairsSerialization<TKey, TValue> : SsSerialization<List<KeyValuePair<TKey, TValue>>>
{
    protected abstract Func<string, TKey> ReadKey { get; }

    protected abstract Func<string, TValue> ReadValue { get; }

    protected abstract Func<TKey, string> WriteKey { get; }

    protected abstract Func<TValue, string> WriteValue { get; }

    public KeyValuePairsSerialization()
    {
        OnSerialize += Serialize;
        OnDeserialize += Deserialize;
    }

    private void Serialize()
    {
        foreach (var (key, value) in Source)
            WriteTag(WriteKey(key), WriteValue(value));
    }

    private void Deserialize()
    {
        Deserialize(typeof(TagValues), token => {
            Source.Add(new(ReadKey(token.Name.Text), ReadValue(((TagValues)token).Tag.Text)));
        });
    }
}
