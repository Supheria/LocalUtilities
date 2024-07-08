using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeGeneral;
using System.Collections.Concurrent;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class ServerHost : Host
{
    public IocpEventHandler<CommandReceiver>? OnOperate;

    public IocpEventHandler? OnClearUp;

    ConcurrentDictionary<ProtocolTypes, ServerProtocol> Protocols { get; } = [];

    public string UserName => UserInfo?.Name ?? "";

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
            protocol.OnOperate += DoOperate;
            protocol.OnOperateCallback += ReceiveOperateCallback;
        }
        else if (protocol.Type is ProtocolTypes.HeartBeats)
            protocol.OnClosed += CloseAll;
        return true;
    CLOSE:
        protocol.Close();
        return false;
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

    public void DoOperate(CommandReceiver receiver)
    {
        var sender = new CommandSender(receiver.TimeStamp, CommandTypes.OperateCallback, receiver.OperateType);
        try
        {
            switch (receiver.OperateType)
            {
                case OperateTypes.Message:
                    DoMessage(receiver);
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
            if (Protocols.TryGetValue(ProtocolTypes.Operator, out var protocol))
                protocol.CallbackFailure(sender, ex);
        }
    }

    private void DoMessage(CommandReceiver receiver)
    {
        var userName = receiver.GetArgs(ProtocolKey.ReceiveUser);
        if (userName != UserName)
        {
            OnOperate?.Invoke(receiver);
            var sender = new CommandSender(receiver.TimeStamp, CommandTypes.OperateCallback, receiver.OperateType);
            Protocols[ProtocolTypes.Operator].CallbackSuccess(sender);
        }
        else
        {
            HandleMessage(receiver);
            var data = receiver.Data;
            var sender = new CommandSender(DateTime.Now, CommandTypes.Operate, receiver.OperateType, data, 0, data.Length)
                .AppendArgs(ProtocolKey.ReceiveUser, receiver.GetArgs(ProtocolKey.ReceiveUser))
                .AppendArgs(ProtocolKey.SendUser, receiver.GetArgs(ProtocolKey.SendUser));
            Protocols[ProtocolTypes.Operator].SendCommand(sender);
        }
        // TODO: make callback by receive user's client
    }

    private void ReceiveOperateCallback(CommandReceiver receiver)
    {

    }

    public void SendMessage(string message)
    {
        try
        {
            var count = WriteU8Buffer(message, out var data);
            var sender = new CommandSender(DateTime.Now, CommandTypes.Operate, OperateTypes.Message, data, 0, count)
                .AppendArgs(ProtocolKey.ReceiveUser, UserName)
                .AppendArgs(ProtocolKey.SendUser, StringTable.Host);
            Protocols[ProtocolTypes.Operator].SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void UpdateUserList(string[] userList)
    {
        try
        {
            var count = WriteU8Buffer(userList.ToArrayString(), out var data);
            var sender = new CommandSender(DateTime.Now, CommandTypes.Operate, OperateTypes.UpdateUserList, data, 0, count);
            Protocols[ProtocolTypes.Operator].SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
}
