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
    protected delegate void CommandHandler(CommandParser commandParser, byte[] buffer, int offset, int count);

    protected Dictionary<CommandTypes, CommandHandler> Commands { get; } = [];

    ConcurrentDictionary<string, OperateSendArgs> CommandWaitList { get; } = [];

    public Protocol()
    {
        Commands[CommandTypes.Operate] = Operate;
        Commands[CommandTypes.OperateCallback] = OperateCallback;
    }

    protected abstract void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count);

    public void SendCommand(CommandTypes type, OperateSendArgs sendArgs)
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
            var commandComposer = new CommandComposer()
                .AppendCommand(type)
                .AppendValue(ProtocolKey.SendArgs, sendArgs.ToSs());
            WriteCommand(commandComposer);
            SendAsync();
        }
    }

    private void Operate(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        var sendArgs = new OperateSendArgs();
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.SendArgs, out var args))
                throw new IocpException(ProtocolCode.ParameterError, nameof(Operate));
            sendArgs.ParseSs(args);
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
            var commandComposer = new CommandComposer()
                .AppendCommand(CommandTypes.OperateCallback)
                .AppendValue(ProtocolKey.CallbackArgs, callbackArgs.ToSs());
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        
    }

    private void OperateCallback(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.CallbackArgs, out var args))
                throw new IocpException(ProtocolCode.ParameterError, nameof(OperateCallback));
            var callbackArgs = new OperateCallbackArgs().ParseSs(args);
            if (!CommandWaitList.TryGetValue(callbackArgs.TimeStamp, out var sendArgs))
                return;
            sendArgs.Waste();
            HandleCallbackCode(sendArgs.Data, callbackArgs.CallbackCode, callbackArgs.ErrorMessage);
            if (callbackArgs.CallbackCode is not ProtocolCode.Success)
                return;
            OnOperateCallback?.Invoke(callbackArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    protected void WriteCommand(CommandComposer commandComposer)
    {
        WriteCommand(commandComposer, [], 0, 0);
    }

    protected void WriteCommand(CommandComposer commandComposer, byte[] buffer, int offset, int count)
    {
        // 获取命令的字节数组
        var commandLength = commandComposer.GetCommandBuffer(out var commandBuffer);
        // 获取总大小(4个字节的包总长度+4个字节的命令长度+命令字节数组的长度+数据的字节数组长度)
        int totalLength = sizeof(int) + sizeof(int) + commandLength + count;
        SendBuffer.StartPacket();
        SendBuffer.DynamicBufferManager.WriteValue(totalLength, false); // 写入总大小
        SendBuffer.DynamicBufferManager.WriteValue(commandLength, false); // 写入命令大小
        SendBuffer.DynamicBufferManager.WriteData(commandBuffer); // 写入命令内容
        SendBuffer.DynamicBufferManager.WriteData(buffer, offset, count); // 写入二进制数据
        SendBuffer.EndPacket();
    }

    protected void HandleCallbackCode(string data, ProtocolCode code, string? errorMessage)
    {
        var log = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(code)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(data)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(errorMessage)
            .ToString();
        HandleLog(log);
    }
}
