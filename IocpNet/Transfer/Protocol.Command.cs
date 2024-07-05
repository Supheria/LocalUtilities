using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    protected delegate void CommandHandler(Command command, byte[] buffer, int offset, int count);

    protected Dictionary<CommandTypes, CommandHandler> Commands { get; } = [];

    ConcurrentDictionary<string, OperateSendArgs> CommandWaitList { get; } = [];

    public Protocol()
    {
        Commands[CommandTypes.Operate] = Operate;
        Commands[CommandTypes.OperateCallback] = OperateCallback;
    }

    protected abstract void ProcessCommand(Command command, byte[] buffer, int offset, int count);

    public void SendCommand(CommandTypes type, OperateArgs args)
    {
        SendCommand(type, args, [], 0, 0);
    }

    public void SendCommand(CommandTypes type, OperateArgs args, byte[] buffer, int offset, int count)
    {
        var commandComposer = new Command(type)
                .AppendOperateArgs(args);
        WriteCommand(commandComposer, buffer, offset, count);
        SendAsync();
    }

    public void SendCommandInWaiting(CommandTypes type, OperateSendArgs sendArgs)
    {
        SendCommandInWaiting(type, sendArgs, [], 0, 0);
    }

    public void SendCommandInWaiting(CommandTypes type, OperateSendArgs sendArgs, byte[] buffer, int offset, int count)
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
            var commandComposer = new Command(type)
                .AppendOperateArgs(sendArgs);
            WriteCommand(commandComposer, buffer, offset, count);
            SendAsync();
        }
    }

    protected bool ReceiveCallback(Command command, out OperateCallbackArgs callbackArgs)
    {
        callbackArgs = command.GetOperateCallbackArgs();
        if (!CommandWaitList.TryGetValue(callbackArgs.TimeStamp, out var sendArgs))
            return false;
        sendArgs.Waste();
        if (callbackArgs.CallbackCode is ProtocolCode.Success)
            return true;
        HandleErrorCode(sendArgs.Type, callbackArgs);
        return false;
    }

    private void Operate(Command command, byte[] buffer, int offset, int count)
    {
        var sendArgs = new OperateSendArgs();
        try
        {
            sendArgs = command.GetOperateSendArgs();
            OnOperate?.Invoke(sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var errorCode = ex switch
            {
                IocpException iocp => iocp.ErrorCode,
                _ => ProtocolCode.UnknowError,
            };
            var callbackArgs = new OperateCallbackArgs(sendArgs.TimeStamp, errorCode, ex.Message);
            OperateCallback(callbackArgs);
        }
    }

    public void OperateCallback(OperateCallbackArgs callbackArgs)
    {
        try
        {
            var command = new Command(CommandTypes.OperateCallback)
                .AppendOperateArgs(callbackArgs);
            WriteCommand(command);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        
    }

    private void OperateCallback(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (ReceiveCallback(command, out var callbackArgs))
                OnOperateCallback?.Invoke(callbackArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    protected void WriteCommand(Command command)
    {
        WriteCommand(command, [], 0, 0);
    }

    protected void WriteCommand(Command command, byte[] buffer, int offset, int count)
    {
        // 获取命令的字节数组
        var commandLength = command.ToSs(out var commandBuffer);
        var str = command.ToSs();
        // 获取总大小(4个字节的包总长度+4个字节的命令长度+命令字节数组的长度+数据的字节数组长度)
        int totalLength = sizeof(int) + sizeof(int) + commandLength + count;
        SendBuffer.StartPacket();
        SendBuffer.DynamicBufferManager.WriteValue(totalLength, false); // 写入总大小
        SendBuffer.DynamicBufferManager.WriteValue(commandLength, false); // 写入命令大小
        SendBuffer.DynamicBufferManager.WriteData(commandBuffer); // 写入命令内容
        SendBuffer.DynamicBufferManager.WriteData(buffer, offset, count); // 写入二进制数据
        SendBuffer.EndPacket();
    }

    protected void HandleErrorCode(OperateTypes type, OperateCallbackArgs callbackArgs)
    {
        var log = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(callbackArgs.CallbackCode)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(type)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(callbackArgs.ErrorMessage)
            .ToString();
        HandleLog(log);
    }
}
