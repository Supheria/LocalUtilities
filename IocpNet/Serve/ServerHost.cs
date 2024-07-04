using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.SimpleScript.Serialization;
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
        return true;
    CLOSE:
        protocol.Close();
        return false;
    }

    private void ReceiveOperate(OperateSendArgs sendArgs)
    {
        switch (sendArgs.Type)
        {
            case OperateTypes.Message:
                ReceiveMessage(sendArgs);
                break;
        }
    }

    private void ReceiveMessage(OperateSendArgs sendArgs)
    {
        try
        {
            HandleLog(sendArgs.Data);
            var protocol = Protocols[ProtocolTypes.Operator];
            var callbackArgs = new OperateCallbackArgs(sendArgs.TimeStamp, ProtocolCode.Success);
            protocol.OperateCallback(callbackArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void ReceiveOperateCallback(OperateCallbackArgs args)
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
            var sendArgs = new OperateSendArgs(OperateTypes.Message, message);
            var protocol = Protocols[ProtocolTypes.Operator];
            protocol.SendCommand(CommandTypes.Operate, sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    //protected override void SendCommand(CommandSendArgs sendArgs)
    //{
    //    try
    //    {
    //        if (!Protocols.TryGetValue(sendArgs.ProtocolType, out var protocol))
    //            throw IocpException.ArgumentNull(nameof(ProtocolTypes));
    //        protocol.Operate(sendArgs);
    //    }
    //    catch (Exception ex)
    //    {
    //        HandleException(ex);
    //    }
    //}

    //protected override void HandleDownloadRequest(CommandReceiveArgs receiveArgs)
    //{
    //    try
    //    {
    //        if (!Protocols.TryGetValue(ProtocolTypes.Download, out var protocol))
    //            throw IocpException.ArgumentNull(nameof(ProtocolTypes));
    //        var requestArgs = new DownloadRequestArgs().ParseSsString(receiveArgs.Arg);
    //        var continueArgs = protocol.StartDownloadContinue(requestArgs.DirName, requestArgs.FileName, requestArgs.StartTime);
    //        var sendArgs = new CommandSendArgs(OperateTypes.DownloadContinue, ProtocolTypes.Download, continueArgs.ToSsString());
    //        //Operate(sendArgs);
    //    }
    //    catch (Exception ex)
    //    {
    //        HandleException(ex);
    //    }
    //}
}
