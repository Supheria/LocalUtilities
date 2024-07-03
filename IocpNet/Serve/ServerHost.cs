using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.TypeGeneral;
using System.Collections.Concurrent;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class ServerHost
{
    public IocpEventHandler? OnClearUp;

    public LogHandler? OnLog;

    ConcurrentDictionary<ProtocolTypes, ServerProtocol> Protocols { get; } = [];

    UserInfo? UserInfo { get; set; } = null;

    public string Name => UserInfo?.Name ?? "";

    public int Count => Protocols.Count;

    public bool Add(ServerProtocol protocol)
    {
        if (protocol.UserInfo is null || (UserInfo is not null && protocol.UserInfo != UserInfo))
            goto CLOSE;
        UserInfo = protocol.UserInfo;
        if (Protocols.TryGetValue(protocol.Type, out var toCheck) && toCheck.TimeStamp != protocol.TimeStamp)
            goto CLOSE;
        if (!Protocols.TryAdd(protocol.Type, protocol))
            goto CLOSE;
        protocol.OnLog += HandleLog;
        if (protocol.Type is ProtocolTypes.Operator)
            protocol.OnOperate += ProcessOperate;
        return true;
    CLOSE:
        protocol.Close();
        return false;
    }

    private void ProcessOperate(OperateReceiveArgs args)
    {
        switch (args.Type)
        {
            case OperateTypes.Message:
                HandleLog(args.Arg);
                return;
        }
    }

    public void Remove(ServerProtocol protocol)
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
        if (!Protocols.TryGetValue(ProtocolTypes.Operator, out var protocol))
            return;
        protocol.Operate(operate, args);
    }

    public void SendMessage(string message)
    {
        Operate(OperateTypes.Message, message);
    }

    public void HandleLog(string log)
    {
        log = new StringBuilder()
            .Append(UserInfo?.Name)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(log)
            .ToString();
        OnLog?.Invoke(log);
    }
}
