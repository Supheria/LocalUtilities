using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public class FileTransferArgs(string dirName, string fileName) : ISsSerializable
{
    public DateTime StartTime { get; private set; } = DateTime.Now;

    public string DirName { get; private set; } = dirName;

    public string FileName { get; private set; } = fileName;

    public string Md5Value { get; set; } = "";

    public long FileLength { get; set; } = 0;

    public long PacketLength { get; set; } = 0;

    public long FilePosition { get; set; } = 0;

    public string LocalName => nameof(FileTransferArgs);

    public FileTransferArgs() : this("", "")
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(StartTime), StartTime.ToBinary().ToString());
        serializer.WriteTag(nameof(DirName), DirName);
        serializer.WriteTag(nameof(FileName), FileName);
        serializer.WriteTag(nameof(Md5Value), Md5Value);
        serializer.WriteTag(nameof(FileLength), FileLength.ToString());
        serializer.WriteTag(nameof(PacketLength), PacketLength.ToString());
        serializer.WriteTag(nameof(FilePosition), FilePosition.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        StartTime = deserializer.ReadTag(nameof(StartTime), s => DateTime.FromBinary(s.ToLong()));
        DirName = deserializer.ReadTag(nameof(DirName));
        FileName = deserializer.ReadTag(nameof(FileName));
        Md5Value = deserializer.ReadTag(nameof(Md5Value));
        FileLength = deserializer.ReadTag(nameof(FileLength), s => s.ToLong());
        PacketLength = deserializer.ReadTag(nameof(PacketLength), s => s.ToInt());
        FilePosition = deserializer.ReadTag(nameof(FilePosition), s => s.ToLong());
    }
}
