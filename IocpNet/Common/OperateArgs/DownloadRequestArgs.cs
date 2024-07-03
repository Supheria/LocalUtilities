using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public class DownloadRequestArgs(string dirName, string fileName, long packetLength, bool canRename) : ISsSerializable
{
    public string DirName { get; private set; } = dirName;

    public string FileName { get; private set; } = fileName;

    public string StartTime { get; private set; } = DateTime.Now.ToString(DateTimeFormat.Data);

    public long PacketLength { get; private set; } = packetLength;

    public bool CanRename { get; private set; } = canRename;

    public string LocalName => nameof(DownloadRequestArgs);

    public DownloadRequestArgs() : this("", "", 0, false)
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(DirName), DirName);
        serializer.WriteTag(nameof(FileName), FileName);
        serializer.WriteTag(nameof(StartTime), StartTime);
        serializer.WriteTag(nameof(PacketLength), PacketLength.ToString());
        serializer.WriteTag(nameof(CanRename), CanRename.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        DirName = deserializer.ReadTag(nameof(DirName));
        FileName = deserializer.ReadTag(nameof(FileName));
        StartTime = deserializer.ReadTag(nameof(StartTime));
        PacketLength = deserializer.ReadTag(nameof(PacketLength), s => s.ToLong());
        CanRename = deserializer.ReadTag(nameof(CanRename), s => s.ToBool());
    }
}
