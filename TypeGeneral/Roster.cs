using LocalUtilities.SimpleScript;
using System.Collections;

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
        return RosterMap.Values.GetEnumerator();
    }
}
