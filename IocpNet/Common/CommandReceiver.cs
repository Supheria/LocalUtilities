﻿using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.IocpNet.Common;

public sealed class CommandReceiver : Command
{
    public CommandReceiver(byte[] packet, out int packetLength)
    {
        packetLength = BitConverter.ToInt32(packet, 0);
        var offset = sizeof(int);
        var argsLength = BitConverter.ToInt32(packet, offset);
        offset += sizeof(int);
        CommandCode = packet[offset++];
        OperateCode = packet[offset++];
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

    public string GetArgs(string key)
    {
        return Args[key];
    }

    public T GetArgs<T>(string key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(Args[key]);
    }
}