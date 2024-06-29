using LocalUtilities.IocpNet.Protocol;

namespace LocalUtilities.IocpNet.Serve;

public class HostProtocolPool(int capacity)
{
    Stack<HostProtocol> Pool { get; } = new(capacity);

    public void Push(HostProtocol item)
    {
        lock (Pool)
            Pool.Push(item);
    }

    public HostProtocol Pop()
    {
        lock (Pool)
            return Pool.Pop();
    }

    public int Count => Pool.Count;
}
