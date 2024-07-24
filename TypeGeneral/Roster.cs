﻿using LocalUtilities.SimpleScript;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeGeneral;

public abstract class Roster<TSignature, TItem>() : ICollection<TItem> where TSignature : notnull where TItem : IRosterItem<TSignature>
{
    protected ConcurrentDictionary<TSignature, TItem> RosterMap { get; set; } = [];

    [SsIgnore]
    public TItem this[TSignature signature]
    {
        get => RosterMap[signature];
        set => RosterMap[signature] = value;
    }

    public int Count => RosterMap.Count;

    public bool IsReadOnly => false;

    public bool TryAdd(TItem item)
    {
        return RosterMap.TryAdd(item.Signature, item);
    }

    public bool TryGetValue(TSignature signature, [NotNullWhen(true)] out TItem? value)
    {
        return RosterMap.TryGetValue(signature, out value);
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
        RosterMap.Values.ToArray().CopyTo(array, arrayIndex);
    }

    public bool TryRemove(TItem item)
    {
        return RosterMap.TryRemove(item.Signature, out _);
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        return RosterMap.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return RosterMap.Values.GetEnumerator();
    }

    [Obsolete]
    public void Add(TItem item)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public bool Remove(TItem item)
    {
        throw new NotImplementedException();
    }
}
