using LocalUtilities.IocpNet.Protocol;
using System.Collections;

namespace LocalUtilities.IocpNet.Serve;

public class HostProtocolList : IList<HostProtocol>
{
    List<HostProtocol> List { get; } = [];

    public int Count => List.Count;

    public bool IsReadOnly { get; } = false;

    public HostProtocol this[int index]
    {
        get => List[index];
        set
        {
            lock (List)
                List[index] = value;
        }
    }

    public void Add(HostProtocol item)
    {
        lock (List)
            List.Add(item);
    }

    public void Remove(HostProtocol item)
    {
        lock (List)
            List.Remove(item);
    }

    public void CopyTo(out HostProtocol[] array)
    {
        lock (List)
        {
            array = new HostProtocol[List.Count];
            List.CopyTo(array);
        }
    }

    public void CopyTo(HostProtocol[] array, int arrayIndex)
    {
        lock (List)
            List.CopyTo(array, arrayIndex);
    }

    public void Clear()
    {
        lock (List)
            List.Clear();
    }

    public int IndexOf(HostProtocol item)
    {
        lock (List)
            return List.IndexOf(item);
    }

    public void Insert(int index, HostProtocol item)
    {
        lock (List)
            List.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        lock (List)
            List.RemoveAt(index);
    }

    public bool Contains(HostProtocol item)
    {
        lock (List)
            return List.Contains(item);
    }

    bool ICollection<HostProtocol>.Remove(HostProtocol item)
    {
        lock (List)
            return List.Remove(item);
    }

    public IEnumerator<HostProtocol> GetEnumerator()
    {
        lock (List)
            return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (List)
            return List.GetEnumerator();
    }
}
