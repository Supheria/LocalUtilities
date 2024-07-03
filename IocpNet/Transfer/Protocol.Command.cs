using LocalUtilities.IocpNet.Common;
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
        Commands[ProtocolKey.Operate] = DoOperate;
        Commands[ProtocolKey.OperateCallback] = DoOperateCallback;
    }

    protected abstract void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count);

    public void Operate(OperateSendArgs operateArgs)
    {
        try
        {
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Operate)
                .AppendValue(ProtocolKey.OperateType, operateArgs.Type)
                .AppendValue(ProtocolKey.TimeStamp, operateArgs.TimeStamp);
            var count = WriteU8Buffer(operateArgs.Arg, out var buffer);
            WriteCommand(commandComposer, buffer, 0, count);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoOperate(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        CommandComposer commandComposer;
        string? timeStamp = null;
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.OperateType, out var operate) ||
                !commandParser.GetValueAsString(ProtocolKey.TimeStamp, out timeStamp))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoOperate));
            var arg = ReadU8Buffer(buffer, offset, count);
            OnOperate?.Invoke(new(operate.ToEnum<OperateTypes>(), arg));
            commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.OperateCallback)
                .AppendValue(ProtocolKey.TimeStamp, timeStamp)
                .AppendSuccess();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.OperateCallback)
                .AppendValue(ProtocolKey.TimeStamp, timeStamp);
            if (ex is IocpException iocp)
                commandComposer.AppendFailure(iocp.ErrorCode, iocp.Message);
            else
                commandComposer.AppendFailure(ProtocolCode.UnknowError, ex.Message);
        }
        WriteCommand(commandComposer);
        SendAsync();
    }

    private void DoOperateCallback(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.TimeStamp, out var timeStamp) ||
                !commandParser.GetValueAsString(ProtocolKey.CallbackCode, out var callbackCode))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoOperateCallback));
            commandParser.GetValueAsString(ProtocolKey.ErrorMessage, out var errorMessage);
            OnOperateCallback?.Invoke(new(timeStamp, callbackCode.ToEnum<ProtocolCode>(), errorMessage));
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
