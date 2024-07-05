using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public class FileProcessArgs(string dirName, string fileName) : ISsSerializable
{
    public DateTime StartTime { get; private set; } = DateTime.Now;

    public string DirName { get; private set; } = dirName;

    public string FileName { get; private set; } = fileName;

    public long FileLength { get; set; } = 0;

    public long PacketLength { get; set; } = 0;

    public long FilePosition { get; set; } = 0;

    public string LocalName => nameof(FileProcessArgs);

    public FileProcessArgs() : this("", "")
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(StartTime), StartTime.ToString(DateTimeFormat.Data));
        serializer.WriteTag(nameof(DirName), DirName);
        serializer.WriteTag(nameof(FileName), FileName);
        serializer.WriteTag(nameof(FileLength), FileLength.ToString());
        serializer.WriteTag(nameof(PacketLength), PacketLength.ToString());
        serializer.WriteTag(nameof(FilePosition), FilePosition.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        StartTime = deserializer.ReadTag(nameof(StartTime), s => s.ToDateTime(DateTimeFormat.Data));
        DirName = deserializer.ReadTag(nameof(DirName));
        FileName = deserializer.ReadTag(nameof(FileName));
        FileLength = deserializer.ReadTag(nameof(FileLength), s => s.ToLong());
        PacketLength = deserializer.ReadTag(nameof(PacketLength), s=>s.ToInt());
        FilePosition = deserializer.ReadTag(nameof(FilePosition), s => s.ToLong());
    }
}
