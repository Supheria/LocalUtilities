namespace LocalUtilities.Interface;

public abstract class Roster<T> where T : IRosterItem
{
    Dictionary<string, T> _roster = [];

    public T[] RosterList => _roster.Values.ToArray();

    public T? this[string name]
    {
        get
        {
            _ = _roster.TryGetValue(name, out var item);
            return item;
        }
        set
        {
            if (name is "" || value is null)
                return;
            _roster[name] = value;
        }
    }

    public void SetRoster(T[] items)
    {
        _roster = [];
        foreach (var item in items)
            _roster[item.Name] = item;
    }

    public void Remove(string name) => _roster.Remove(name);
}
