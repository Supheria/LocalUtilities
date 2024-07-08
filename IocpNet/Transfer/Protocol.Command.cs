using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    protected delegate void CommandHandler(CommandReceiver receiver);

    protected Dictionary<CommandTypes, CommandHandler> Commands { get; } = [];

    ConcurrentDictionary<DateTime, CommandSender> CommandWaitList { get; } = [];

    public Protocol()
    {
        Commands[CommandTypes.Operate] = Operate;
        Commands[CommandTypes.OperateCallback] = OperateCallback;
    }

    protected ValidateHandler FileValdate { get; } = (filePath) =>
    {
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var md5 = fileStream.ToMd5HashString();
        fileStream.Dispose();
        return md5;
    };

    protected abstract void ProcessCommand(CommandReceiver receiver);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="doRetry"></param>
    /// <exception cref="IocpException"></exception>
    public void SendCommand(CommandSender sender)
    {
        sender.OnLog += HandleLog;
        if (sender.NeedWaitingCallback)
        {
            sender.OnWasted += () => CommandWaitList.TryRemove(sender.TimeStamp, out _);
            sender.OnRetry += sendCommend;
            if (!CommandWaitList.TryAdd(sender.TimeStamp, sender))
                throw new IocpException(ProtocolCode.CannotAddSendCommand);
            sender.StartWaitingCallback();
        }
        sendCommend();
        void sendCommend()
        {
            var packet = sender.GetPacket();
            SendBuffer.WriteData(packet, 0, packet.Length);
            SendAsync();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="receiver"></param>
    /// <returns></returns>
    /// <exception cref="IocpException"></exception>
    protected void ReceiveCallback(CommandReceiver receiver)
    {
        if (receiver.NeedWaitingCallback)
        {
            if (!CommandWaitList.TryGetValue(receiver.TimeStamp, out var commandSend))
                throw new IocpException(ProtocolCode.CannotFindSourceSendCommand);
            commandSend.Waste();
        }
        var callbackCode = receiver.GetCallbackCode();
        if (callbackCode is not ProtocolCode.Success)
            throw new IocpException(callbackCode, receiver.GetErrorMessage());
    }

    private void Operate(CommandReceiver receiver)
    {
        OnOperate?.Invoke(receiver);
    }

    private void OperateCallback(CommandReceiver receiver)
    {
        try
        {
            ReceiveCallback(receiver);
            OnOperateCallback?.Invoke(receiver);
        }
        catch (Exception ex)
        {
            HandleException(nameof(OperateCallback), ex);
        }
    }
}
