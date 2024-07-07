using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    protected delegate void CommandHandler(Command command);

    protected Dictionary<CommandTypes, CommandHandler> Commands { get; } = [];

    ConcurrentDictionary<DateTime, CommandSend> CommandWaitList { get; } = [];

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

    protected abstract void ProcessCommand(Command command);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandSend"></param>
    /// <param name="doRetry"></param>
    /// <exception cref="IocpException"></exception>
    public void SendCommand(CommandSend commandSend, bool doRetry)
    {
        commandSend.OnLog += HandleLog;
        commandSend.OnWasted += () => CommandWaitList.TryRemove(commandSend.TimeStamp, out _);
        if (doRetry)
            commandSend.OnRetry += sendCommend;
        if (!CommandWaitList.TryAdd(commandSend.TimeStamp, commandSend))
            throw new IocpException(ProtocolCode.CannotAddSendCommand);
        sendCommend();
        void sendCommend()
        {
            WriteCommand(commandSend);
            SendAsync();
        }
    }

    public void SendCallback(CommandCallback commandCallback)
    {
        WriteCommand(commandCallback);
        SendAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="IocpException"></exception>
    protected void ReceiveCallback(Command command)
    {
        if (command is not CommandCallback commandCallback)
            throw new IocpException(ProtocolCode.ArgumentError);
        if (!CommandWaitList.TryGetValue(commandCallback.TimeStamp, out var commandSend))
            throw new IocpException(ProtocolCode.CannotFindSourceSendCommand);
        commandSend.Waste();
        var callbackCode = commandCallback.GetCallbackCode();
        if (callbackCode is not ProtocolCode.Success)
            throw new IocpException(callbackCode, commandCallback.GetErrorMessage());
    }

    private void Operate(Command command)
    {
        OnOperate?.Invoke(command);
    }

    private void OperateCallback(Command command)
    {
        try
        {
            ReceiveCallback(command);
            OnOperateCallback?.Invoke(command);
        }
        catch (Exception ex)
        {
            HandleException(nameof(OperateCallback), ex);
        }
    }

    private void WriteCommand(Command command)
    {
        var packet = command.GetPacket();
        SendBuffer.WriteData(packet, 0, packet.Length);
    }
}
