﻿using LocalUtilities.SimpleScript.Serialization;
using System.Collections;

namespace LocalUtilities.TypeGeneral;

public abstract class Roster<TSignature, TItem>() : ISsSerializable, ICollection<TItem>, IEnumerable where TSignature : notnull where TItem : RosterItem<TSignature>, new()
{
    public abstract string LocalName { get; }

    protected Dictionary<TSignature, TItem> RosterMap { get; set; } = [];

    public List<TItem> RosterList
    {
        get => RosterMap.Values.ToList();
        set
        {
            RosterMap.Clear();
            foreach (var item in value)
                RosterMap[item.Signature] = item;
        }
    }

    public TItem? this[TSignature signature]
    {
        get
        {
            _ = RosterMap.TryGetValue(signature, out var item);
            return item;
        }
        set
        {
            if (signature is "" || value is null)
                return;
            RosterMap[signature] = value;
        }
    }

    protected abstract void SerializeRoster(SsSerializer serializer);

    protected abstract void DeserializeRoster(SsDeserializer deserializer);

    public void Serialize(SsSerializer serializer)
    {
        SerializeRoster(serializer);
        serializer.WriteObjects(LocalName, RosterMap.Values);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        DeserializeRoster(deserializer);
        RosterList = deserializer.ReadObjects<TItem>(LocalName);
    }

    public int Count => RosterMap.Count;

    public bool IsReadOnly => true;

    public void Add(TItem item)
    {
        RosterMap[item.Signature] = item;
    }

    public virtual void Remove(TSignature signature)
    {
        RosterMap.Remove(signature);
    }

    public void Clear()
    {
        RosterMap.Clear();
    }

    public bool Contains(TItem item)
    {
        return RosterMap.ContainsKey(item.Signature);
    }

    public void CopyTo(TItem[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TItem item)
    {
        return RosterMap.Remove(item.Signature);
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        return RosterMap.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
