using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public class DownloadConfirmArgs(long fileLength, long packetLength, long filePosition, string startTime) : ISsSerializable
{
    public long FileLength { get; private set; } = fileLength;

    public long PacketLength { get; private set; } = packetLength;

    public long FilePosition { get; private set; } = filePosition;

    public string StartTime { get; private set; } = startTime;

    public string LocalName => nameof(DownloadConfirmArgs);

    public DownloadConfirmArgs(): this(0, 0, 0, "")
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(FileLength), FileLength.ToString());
        serializer.WriteTag(nameof(PacketLength), PacketLength.ToString());
        serializer.WriteTag(nameof(FilePosition), FilePosition.ToString());
        serializer.WriteTag(nameof(StartTime), StartTime);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        FileLength = deserializer.ReadTag(nameof(FileLength), s => s.ToLong());
        PacketLength = deserializer.ReadTag(nameof(PacketLength), s=>s.ToInt());
        FilePosition = deserializer.ReadTag(nameof(FilePosition), s => s.ToLong());
        StartTime = deserializer.ReadTag(nameof(StartTime));
    }
}
