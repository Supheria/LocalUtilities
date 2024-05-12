using System.Collections;

namespace LocalUtilities.Interface;

public abstract class Roster<TSignature, TItem> : ICollection<TItem>, IEnumerable where TSignature : notnull where TItem : RosterItem<TSignature>
{
    protected Dictionary<TSignature, TItem> RosterMap { get; set; } = [];

    public List<TItem> GetRosterList()
    {
        return RosterMap.Values.ToList();
    }

    public int Count => RosterMap.Count;

    public bool IsReadOnly => true;

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

    public void Add(TItem item)
    {
        RosterMap.Add(item.Signature, item);
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
        return RosterMap.GetEnumerator();
    }
}
