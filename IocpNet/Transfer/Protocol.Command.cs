using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    protected delegate void CommandHandler(CommandParser commandParser, byte[] buffer, int offset, int count);

    protected Dictionary<ProtocolKey, CommandHandler> Commands { get; } = [];

    public Protocol()
    {
        Commands[ProtocolKey.Operate] = Command;
        Commands[ProtocolKey.OperateCallback] = CommandCallback;
    }

    protected abstract void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count);

    public void Command(CommandSendArgs sendArgs)
    {
        try
        {
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Operate)
                .AppendValue(ProtocolKey.SendArgs, sendArgs.ToSsString());
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void Command(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        var sendArgs = new CommandSendArgs();
        CommandCallbackArgs callbackArgs;
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.SendArgs, out var args))
                throw new IocpException(ProtocolCode.ParameterError, nameof(Command));
            sendArgs.ParseSsString(args);
            //sendArgs.ParseSsBuffer(buffer, offset, count);
            OnOperate?.Invoke(new(sendArgs.OperateType, sendArgs.Data));
            callbackArgs = new(sendArgs.TimeStamp, ProtocolCode.Success);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var errorCode = ex switch
            {
                IocpException iocp => iocp.ErrorCode,
                _ => ProtocolCode.UnknowError,
            };
            callbackArgs = new(sendArgs.TimeStamp, errorCode, ex.Message);
        }
        var commandComposer = new CommandComposer()
            .AppendCommand(ProtocolKey.OperateCallback);
        count = callbackArgs.ToSsBuffer(out buffer);
        WriteCommand(commandComposer, buffer, 0, count);
        SendAsync();
    }

    private void CommandCallback(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            var callbackArgs = new CommandCallbackArgs().ParseSsBuffer(buffer, offset, count);
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
        // 获取命令
        var command = commandComposer.GetCommand();
        // 获取命令的字节数组
        var commandLength = WriteU8Buffer(command, out var commandBuffer);
        // 获取总大小(4个字节的包总长度+4个字节的命令长度+命令字节数组的长度+数据的字节数组长度)
        int totalLength = sizeof(int) + sizeof(int) + commandLength + count;
        SendBuffer.StartPacket();
        SendBuffer.DynamicBufferManager.WriteValue(totalLength, false); // 写入总大小
        SendBuffer.DynamicBufferManager.WriteValue(commandLength, false); // 写入命令大小
        SendBuffer.DynamicBufferManager.WriteData(commandBuffer); // 写入命令内容
        SendBuffer.DynamicBufferManager.WriteData(buffer, offset, count); // 写入二进制数据
        SendBuffer.EndPacket();
    }

    protected static int WriteU8Buffer(string? str, [NotNullWhen(true)] out byte[] buffer)
    {
        buffer = [];
        try
        {
            if (str is null)
                return 0;
            buffer = Encoding.UTF8.GetBytes(str);
            return buffer.Length;
        }
        catch
        {
            return 0;
        }
    }

    protected static string ReadU8Buffer(byte[] buffer, int offset, int count)
    {
        try
        {
            return Encoding.UTF8.GetString(buffer, offset, count);
        }
        catch
        {
            return "";
        }
    }
}
