using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using System.Collections.Concurrent;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class ServerHost : Host
{
    public IocpEventHandler? OnClearUp;

    ConcurrentDictionary<ProtocolTypes, ServerProtocol> Protocols { get; } = [];

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
        {
            protocol.OnOperate += ReceiveOperate;
            protocol.OnOperateCallback += ReceiveOperateCallback;
        }
        else if (protocol.Type is ProtocolTypes.HeartBeats)
            protocol.OnClosed += CloseAll;
        return true;
    CLOSE:
        protocol.Close();
        return false;
    }

    private void ReceiveOperate(Command command)
    {
        var sendArgs = command.GetOperateSendArgs();
        switch (sendArgs.Type)
        {
            case OperateTypes.Message:
                ReceiveMessage(sendArgs, command.Data);
                break;
        }
    }

    private void ReceiveMessage(OperateSendArgs sendArgs, byte[] data)
    {
        try
        {
            var message = Encoding.UTF8.GetString(data);
            HandleLog(message);
            var protocol = Protocols[ProtocolTypes.Operator];
            var callbackArgs = new OperateCallbackArgs(sendArgs)
                .AppendSuccess();
            protocol.SendCommand(CommandTypes.OperateCallback, callbackArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void ReceiveOperateCallback(Command command)
    {

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

    public void SendMessage(string message)
    {
        try
        {
            var sendArgs = new OperateSendArgs(OperateTypes.Message);
            var data = Encoding.UTF8.GetBytes(message);
            var protocol = Protocols[ProtocolTypes.Operator];
            protocol.SendCommandInWaiting(CommandTypes.Operate, sendArgs, data, 0, data.Length);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void UpdateUserList(List<string> userList)
    {
        try
        {
            var sendArgs = new OperateSendArgs(OperateTypes.UserList);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
}
