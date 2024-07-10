using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public class CommandSender : Command
{
    public NetEventHandler? OnWaitingCallbackFailed;

    public event NetEventHandler? OnWasted;

    DaemonThread? DaemonThread { get; set; }

    public CommandSender(DateTime timeStamp, byte commandCode, byte operateCode, byte[] data, int dataOffset, int dataCount)
    {
        TimeStamp = timeStamp;
        CommandCode = commandCode;
        OperateCode = operateCode;
        Data = new byte[dataCount];
        Array.Copy(data, dataOffset, Data, 0, dataCount);
    }

    public CommandSender(DateTime timeStamp, byte commandCode, byte operateCode)
    {
        TimeStamp = timeStamp;
        CommandCode = commandCode;
        OperateCode = operateCode;
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
        buffer[offset++] = CommandCode;
        buffer[offset++] = OperateCode;
        Array.Copy(BitConverter.GetBytes(TimeStamp.ToBinary()), 0, buffer, offset, sizeof(long));
        offset += sizeof(long);
        Array.Copy(args, 0, buffer, offset, args.Length);
        offset += args.Length;
        Array.Copy(Data, 0, buffer, offset, Data.Length);
        return buffer;
    }

    public CommandSender AppendArgs(string key, string args)
    {
        Args[key] = args;
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
            .Append(CommandCode)
            .Append(SignTable.Comma)
            .Append(SignTable.Space)
            .Append(OperateCode)
            .ToString();
        this.HandleLog(message);
    }
}
