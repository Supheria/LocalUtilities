using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public abstract class OperateArgs(string timeStamp, string args) : ISsSerializable
{
    protected event SerializeHandler? OnSerialize;

    protected event DeserializeHandler? OnDeserialize;

    public string TimeStamp { get; private set; } = timeStamp;

    public string Args { get; private set; } = args;

    public abstract string LocalName { get; }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
        serializer.WriteTag(nameof(Args), Args);
        OnSerialize?.Invoke(serializer);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
        Args = deserializer.ReadTag(nameof(Args));
        OnDeserialize?.Invoke(deserializer);
    }
}
