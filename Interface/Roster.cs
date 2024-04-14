namespace LocalUtilities.Interface;

public abstract class Roster<TSignature, TItem> where TSignature : notnull where TItem : RosterItem<TSignature>
{
    protected Dictionary<TSignature, TItem> RosterMap { get; set; } = [];

    public TItem[] RosterList
    {
        get => RosterMap.Values.ToArray();
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

    public virtual void Remove(TSignature signature) => RosterMap.Remove(signature);
}
