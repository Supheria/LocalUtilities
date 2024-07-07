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

    private void ReceiveOperate(Command command)
    {
        try
        {
            switch (command.OperateType)
            {
                case OperateTypes.Message:
                    ReceiveMessage(command);
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void ReceiveMessage(Command command)
    {
        var message = ReadU8Buffer(command.Data);
        HandleLog(message);
        var protocol = Protocols[ProtocolTypes.Operator];
        var commandCallback = new CommandCallback(command.TimeStamp, CommandTypes.OperateCallback, command.OperateType)
            .AppendSuccess();
        protocol.SendCallback(commandCallback);
    }

    private void ReceiveOperateCallback(Command command)
    {

    }

    public void SendMessage(string message)
    {
        try
        {
            var count = WriteU8Buffer(message, out var data);
            var commandSend = new CommandSend(CommandTypes.Operate, OperateTypes.Message, data, 0, count);
            var protocol = Protocols[ProtocolTypes.Operator];
            protocol.SendCommand(commandSend, true);
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
            var commandSend = new CommandSend(CommandTypes.Operate, OperateTypes.UserList, data, 0, count);
            var protocol = Protocols[ProtocolTypes.Operator];
            protocol.SendCommand(commandSend, false);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
}
