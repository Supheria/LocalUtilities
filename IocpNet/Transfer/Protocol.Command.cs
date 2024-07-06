using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.TypeGeneral;
using System.Collections.Concurrent;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    protected delegate void CommandHandler(Command command);

    protected Dictionary<CommandTypes, CommandHandler> Commands { get; } = [];

    ConcurrentDictionary<string, OperateSendArgs> CommandWaitList { get; } = [];

    public Protocol()
    {
        Commands[CommandTypes.Operate] = Operate;
        Commands[CommandTypes.OperateCallback] = OperateCallback;
    }
    public void SendCommand(CommandTypes type, OperateArgs args)
    {
        SendCommand(type, args, [], 0, 0);
    }

    public void SendCommand(CommandTypes type, OperateArgs args, byte[] buffer, int offset, int count)
    {
        try
        {
            var command = new Command(type, args, buffer, offset, count);
            WriteCommand(command);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    protected abstract void ProcessCommand(Command command);

    public void SendCommandInWaiting(CommandTypes type, OperateSendArgs sendArgs)
    {
        SendCommandInWaiting(type, sendArgs, [], 0, 0);
    }

    public void SendCommandInWaiting(CommandTypes type, OperateSendArgs sendArgs, byte[] data, int dataOffset, int dataCount)
    {
        try
        {
            sendArgs.OnLog += HandleLog;
            sendArgs.OnRetry += operate;
            sendArgs.OnWasted += () => CommandWaitList.TryRemove(sendArgs.TimeStamp, out _);
            CommandWaitList.TryAdd(sendArgs.TimeStamp, sendArgs);
            operate();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        void operate()
        {
            var command = new Command(type, sendArgs, data, dataOffset, dataCount);
            WriteCommand(command);
            SendAsync();
        }
    }

    protected bool ReceiveCallback(Command command, out OperateCallbackArgs callbackArgs)
    {
        callbackArgs = new();
        try
        {
            callbackArgs = command.GetOperateCallbackArgs();
            if (!CommandWaitList.TryGetValue(callbackArgs.TimeStamp, out var sendArgs))
                return false;
            sendArgs.Waste();
            if (callbackArgs.GetCallbackCode() is ProtocolCode.Success)
                return true;
            HandleErrorCode(callbackArgs);
            return false;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return false;
        }
    }

    private void Operate(Command command)
    {
        OnOperate?.Invoke(command);
    }

    private void OperateCallback(Command command)
    {
        try
        {
            if (ReceiveCallback(command, out _))
                OnOperateCallback?.Invoke(command);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    protected void WriteCommand(Command command)
    {
        SendBuffer.AppendPacket(command.GetPacket());
    }

    protected void HandleErrorCode(OperateCallbackArgs callbackArgs)
    {
        var log = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(callbackArgs.GetCallbackCode())
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(callbackArgs.Type)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(callbackArgs.GetErrorMessage())
            .ToString();
        HandleLog(log);
    }
}
