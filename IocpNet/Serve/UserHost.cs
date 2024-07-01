using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Serve;

public class UserHost
{
    public IocpEventHandler? OnClearUp;

    public LogHandler? OnLog;

    ConcurrentDictionary<IocpProtocolTypes, HostProtocol> Protocols { get; } = [];

    public string Name { get; private set; } = "";

    public int Count => Protocols.Count;

    public bool Add(HostProtocol protocol)
    {
        if (protocol.UserInfo?.Name is null || (Name is not "" && protocol.UserInfo.Name != Name))
            goto CLOSE;
        Name = protocol.UserInfo.Name;
        if (Protocols.TryGetValue(protocol.Type, out var toCheck) && toCheck.TimeStamp != protocol.TimeStamp)
            goto CLOSE;
        if (!Protocols.TryAdd(protocol.Type, protocol))
            goto CLOSE;
        protocol.OnLog += (s) => OnLog?.Invoke(s);
        if (protocol.Type is IocpProtocolTypes.Operator)
            protocol.OnOperate += ProcessOperate;
        return true;
    CLOSE:
        protocol.Close();
        return false;
    }

    private void ProcessOperate(OperateArgs args)
    {
        switch (args.Type)
        {
            case OperateTypes.Message:
                OnLog?.Invoke(args.Args);
                return;
        }
    }

    public void Remove(HostProtocol protocol)
    {
        if (Protocols.TryGetValue(protocol.Type, out var toCheck) && toCheck.TimeStamp == protocol.TimeStamp)
            Protocols.TryRemove(protocol.Type, out _);
        if (Protocols.Count is 0)
            OnClearUp?.Invoke();
    }

    public void CloseAll()
    {
        foreach (var protocol in Protocols.Values)
        {
            lock (protocol)
                protocol.Close();
        }
    }

    public void Operate(OperateTypes operate, string args)
    {
        if (!Protocols.TryGetValue(IocpProtocolTypes.Operator, out var protocol))
            return;
        protocol.Operate(operate, args);
    }

    public void SendMessage(string message)
    {
        Operate(OperateTypes.Message, message);
    }
}
