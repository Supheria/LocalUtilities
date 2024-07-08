using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public sealed class CommandSender : Command
{
    public IocpEventHandler? OnWaitingCallbackFailed;

    public event IocpEventHandler? OnWasted;

    DaemonThread? DaemonThread { get; set; }

    public CommandSender(DateTime timeStamp, CommandTypes commandType, OperateTypes operateType, byte[] data, int dataOffset, int dataCount)
    {
        TimeStamp = timeStamp;
        CommandType = commandType;
        OperateType = operateType;
        Data = new byte[dataCount];
        Array.Copy(data, dataOffset, Data, 0, dataCount);
    }

    public CommandSender(DateTime timeStamp, CommandTypes commandType, OperateTypes operateType)
    {
        TimeStamp = timeStamp;
        CommandType = commandType;
        OperateType = operateType;
    }

    public byte[] GetPacket()
    {
        var args = Args.ToSsBuffer();
        var PacketLength = HeadLength + args.Length + Data.Length;
        var buffer = new byte[PacketLength];
        var offset = 0;
        Array.Copy(BitConverter.GetBytes(PacketLength), 0, buffer, 0, sizeof(int));
        offset += sizeof(int);
        Array.Copy(BitConverter.GetBytes(args.Length), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        buffer[offset++] = (byte)CommandType;
        buffer[offset++] = (byte)OperateType;
        Array.Copy(BitConverter.GetBytes(TimeStamp.ToBinary()), 0, buffer, offset, sizeof(long));
        offset += sizeof(long);
        Array.Copy(args, 0, buffer, offset, args.Length);
        offset += args.Length;
        Array.Copy(Data, 0, buffer, offset, Data.Length);
        return buffer;
    }

    public CommandSender AppendArgs(ProtocolKey key, string args)
    {
        Args[key] = args;
        return this;
    }

    public CommandSender AppendSuccess()
    {
        AppendArgs(ProtocolKey.CallbackCode, ProtocolCode.Success.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, "");
        return this;
    }

    public CommandSender AppendFailure(Exception ex)
    {
        var errorCode = ex switch
        {
            IocpException iocp => iocp.ErrorCode,
            _ => ProtocolCode.UnknowError,
        };
        AppendArgs(ProtocolKey.CallbackCode, errorCode.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, ex.Message);
        return this;
    }

    public void StartWaitingCallback()
    {
        DaemonThread = new(ConstTabel.WaitingCallbackMilliseconds, WaitingFailed);
        DaemonThread.Start();
    }

    private void WaitingFailed()
    {
        Waste();
        OnWaitingCallbackFailed?.Invoke();
        HandleWaitingCallbackFailed();
        return;
    }

    public void Waste()
    {
        DaemonThread?.Dispose();
        OnWasted?.Invoke();
    }

    private void HandleWaitingCallbackFailed()
    {
        var message = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(StringTable.WaitingCallback)
            .Append(SignTable.Space)
            .Append(StringTable.Failed)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(CommandType)
            .Append(SignTable.Comma)
            .Append(SignTable.Space)
            .Append(OperateType)
            .ToString();
        HandleLog(message);
    }
}
