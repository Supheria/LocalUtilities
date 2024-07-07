using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.IocpNet.Transfer;

public class Command
{
    public int PacketLength { get; }

    public CommandTypes Type { get; }

    byte[] Args { get; }

    public byte[] Data { get; }

    public Command(CommandTypes type, OperateArgs? args, byte[] data, int dataOffset, int dataCount)
    {
        Type = type;
        var str = args?.ToSsString();
        Args = args?.ToSsBuffer() ?? [];
        Data = new byte[dataCount];
        Array.Copy(data, dataOffset, Data, 0, dataCount);
        PacketLength
            = sizeof(int) // total length
            + sizeof(int) // args length
            + sizeof(byte) // type
            + Args.Length
            + dataCount;
    }

    public Command(CommandTypes type, OperateArgs? args) : this(type, args, [], 0, 0)
    {

    }

    public Command(byte[] packet)
    {
        var offset = 0;
        PacketLength = BitConverter.ToInt32(packet, offset);
        offset += sizeof(int);
        var argsLength = BitConverter.ToInt32(packet, offset);
        offset += sizeof(int);
        Type = (CommandTypes)packet[offset++];
        Args = new byte[argsLength];
        Array.Copy(packet, offset, Args, 0, argsLength);
        offset += argsLength;
        Data = new byte[PacketLength - offset];
        Array.Copy(packet, offset, Data, 0, Data.Length);
    }

    public byte[] GetPacket()
    {
        var buffer = new byte[PacketLength];
        var offset = 0;
        Array.Copy(BitConverter.GetBytes(PacketLength), 0, buffer, 0, sizeof(int));
        offset += sizeof(int);
        Array.Copy(BitConverter.GetBytes(Args.Length), 0, buffer, offset, sizeof(int));
        offset += sizeof(int);
        buffer[offset++] = (byte)Type;
        Array.Copy(Args, 0, buffer, offset, Args.Length);
        offset += Args.Length;
        Array.Copy(Data, 0, buffer, offset, Data.Length);
        return buffer;
    }

    public static bool FullPacket(byte[] packet)
    {
        if (packet.Length < sizeof(int))
            return false;
        var packetLength = BitConverter.ToInt32(packet, 0);
        return packet.Length >= packetLength;
    }

    public static bool OutOfLimit(byte[] packet)
    {
        var packetLength = BitConverter.ToInt32(packet, 0);
        var argsLength = BitConverter.ToInt32(packet, sizeof(int));
        var dataLenght = packetLength - argsLength - sizeof(int) - sizeof(int) - sizeof(byte);
        return dataLenght > ConstTabel.DataBytesTransferredMax;
    }

    public OperateSendArgs GetOperateSendArgs()
    {
        return new OperateSendArgs().ParseSs(Args, 0, Args.Length);
    }

    public OperateCallbackArgs GetOperateCallbackArgs()
    {
        return new OperateCallbackArgs().ParseSs(Args, 0, Args.Length);
    }
}
