using LocalUtilities.IocpNet.Common;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System.Reflection.Emit;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public sealed class CommandReceiver : Command
{
    public CommandReceiver(byte[] packet, out int packetLength)
    {
        packetLength = BitConverter.ToInt32(packet, 0);
        var offset = sizeof(int);
        var argsLength = BitConverter.ToInt32(packet, offset);
        offset += sizeof(int);
        CommandType = (CommandTypes)packet[offset++];
        OperateType = (OperateTypes)packet[offset++];
        TimeStamp = DateTime.FromBinary(BitConverter.ToInt64(packet, offset));
        offset += sizeof(long);
        Args = new CommandArgs().ParseSs(packet, offset, argsLength);
        offset += argsLength;
        Data = new byte[packetLength - offset];
        Array.Copy(packet, offset, Data, 0, Data.Length);
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

    public string GetArgs(ProtocolKey key)
    {
        return Args[key];
    }

    public T GetArgs<T>(ProtocolKey key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(Args[key]);
    }

    public ProtocolCode GetCallbackCode()
    {
        return GetArgs(ProtocolKey.CallbackCode).ToEnum<ProtocolCode>();
    }

    public string GetErrorMessage()
    {
        return new StringBuilder()
            .Append(SignTable.OpenParenthesis)
            .Append(CommandType)
            .Append(SignTable.Comma)
            .Append(SignTable.Space)
            .Append(OperateType)
            .Append(SignTable.CloseParenthesis)
            .Append(GetArgs(ProtocolKey.ErrorMessage))
            .ToString();
    }
}
