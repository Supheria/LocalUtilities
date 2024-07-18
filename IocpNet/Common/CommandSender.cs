using LocalUtilities.SimpleScript;

namespace LocalUtilities.IocpNet.Common;

public class CommandSender : Command
{
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
        var args = SerializeTool.Serialize(Args, new(), SignTable, null);
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

    public CommandSender AppendArgs(string key, object? obj)
    {
        var str = SerializeTool.Serialize(obj, new(), false, SignTable) ?? "";
        Args[key] = str;
        return this;
    }
}
