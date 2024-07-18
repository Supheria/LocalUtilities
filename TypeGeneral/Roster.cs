using LocalUtilities.SimpleScript;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeGeneral;

public abstract class Roster<TSignature, TItem>() : ICollection<TItem> where TSignature : notnull where TItem : RosterItem<TSignature>
{
    protected Dictionary<TSignature, TItem> RosterMap { get; set; } = [];

    [SsIgnore]
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

    [SsIgnore]
    public TItem this[TSignature signature]
    {
        get => RosterMap[signature];
        set => RosterMap[signature] = value;
    }

    public int Count => RosterMap.Count;

    public bool IsReadOnly => false;

    public void Add(TItem item)
    {
        RosterMap.Add(item.Signature, item);
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
        RosterList.CopyTo(array, arrayIndex);
    }

    public bool Remove(TItem item)
    {
        return RosterMap.Remove(item.Signature);
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        return RosterList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return RosterList.GetEnumerator();
    }
}
