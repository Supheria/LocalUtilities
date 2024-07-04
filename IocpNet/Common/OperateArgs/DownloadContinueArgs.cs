using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public class DownloadContinueArgs(string startTime, long packetLength) : ISsSerializable
{
    public string StartTime { get; private set; } = startTime;

    public long PacketLength { get; private set; } = packetLength;

    public string LocalName => nameof(DownloadContinueArgs);

    public DownloadContinueArgs() : this("", 0)
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(StartTime), StartTime);
        serializer.WriteTag(nameof(PacketLength), PacketLength.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        StartTime = deserializer.ReadTag(nameof(StartTime));
        PacketLength = deserializer.ReadTag(nameof(PacketLength), s => s.ToLong());
    }
}
