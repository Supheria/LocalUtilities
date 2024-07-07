using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public class Command
{
    public event LogHandler? OnLog;

    protected enum Types : byte
    {
        Send,
        Callback,
    }

    Types Type { get; }

    public DateTime TimeStamp { get; private set; } = new();

    public CommandTypes CommandType { get; private set; } = CommandTypes.None;

    public OperateTypes OperateType { get; private set; } = OperateTypes.None;

    CommandArgs Args { get; set; } = new();

    public byte[] Data { get; private set; } = [];

    const int HeadLength
        = sizeof(int) // total length
        + sizeof(int) // args length
        + sizeof(byte) // type
        + sizeof(byte) // command type
        + sizeof(byte) // operate type
        + sizeof(long) // time stamp
        ;

    protected Command(Types type, DateTime timeStamp, CommandTypes commandType, OperateTypes operateType, byte[] data, int dataOffset, int dataCount)
    {
        Type = type;
        TimeStamp = timeStamp;
        CommandType = commandType;
        OperateType = operateType;
        Data = new byte[dataCount];
        Array.Copy(data, dataOffset, Data, 0, dataCount);
    }

    protected Command(Types type, DateTime timeStamp, CommandTypes commandType, OperateTypes operateType)
    {
        Type = type;
        TimeStamp = timeStamp;
        CommandType = commandType;
        OperateType = operateType;
    }

    private Command(Types type)
    {
        Type = type;
    }

    public static Command? GetCommand(byte[] packet, out int packetLength)
    {
        packetLength = BitConverter.ToInt32(packet, 0);
        var offset = sizeof(int);
        var argsLength = BitConverter.ToInt32(packet, offset);
        offset += sizeof(int);
        var type = (Types)packet[offset++];
        var commandType = (CommandTypes)packet[offset++];
        var operateType = (OperateTypes)packet[offset++];
        var timeStamp = DateTime.FromBinary(BitConverter.ToInt64(packet, offset));
        offset += sizeof(long);
        var args = new CommandArgs().ParseSs(packet, offset, argsLength);
        offset += argsLength;
        var data = new byte[packetLength - offset];
        Array.Copy(packet, offset, data, 0, data.Length);
        return type switch
        {
            Types.Send => new Command(type)
            {
                CommandType = commandType,
                OperateType = operateType,
                TimeStamp = timeStamp,
                Args = args,
                Data = data,
            },
            Types.Callback => new CommandCallback(timeStamp, commandType, operateType)
            {
                Args = args,
                Data = data,
            },
            _ => null
        };
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
        buffer[offset++] = (byte)Type;
        buffer[offset++] = (byte)CommandType;
        buffer[offset++] = (byte)OperateType;
        Array.Copy(BitConverter.GetBytes(TimeStamp.ToBinary()), 0, buffer, offset, sizeof(long));
        offset += sizeof(long);
        Array.Copy(args, 0, buffer, offset, args.Length);
        offset += args.Length;
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
        var dataLenght = packetLength - argsLength - HeadLength;
        return dataLenght > ConstTabel.DataBytesTransferredMax;
    }

    public virtual Command AppendArgs(ProtocolKey key, string args)
    {
        Args.Map[key] = args;
        return this;
    }

    public string GetArgs(ProtocolKey key)
    {
        return Args.Map[key];
    }

    public T GetArgs<T>(ProtocolKey key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(Args.Map[key]);
    }

    protected void HandleLog(string message)
    {
        OnLog?.Invoke(message);
    }
}
