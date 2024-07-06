using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public abstract class OperateArgs : SerializableTagValues<ProtocolKey, string>
{
    public OperateTypes Type { get; private set; }

    public string TimeStamp { get; private set; }

    protected override Func<ProtocolKey, string> WriteTag => x => x.ToString();

    protected override Func<string, List<string>> WriteValue => x => [x];

    protected override Func<string, ProtocolKey> ReadTag => s => s.ToEnum<ProtocolKey>();

    protected override Func<List<string>, string> ReadValue => s => s[0];

    public OperateArgs(OperateTypes type, string timeStamp)
    {
        Type = type;
        TimeStamp = timeStamp;
        OnSerialize += OperateArgs_OnSerialize;
        OnDeserialize += OperateArgs_OnDeserialize;
    }

    private void OperateArgs_OnSerialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(Type), Type.ToString());
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
    }

    private void OperateArgs_OnDeserialize(SsDeserializer deserializer)
    {
        Type = deserializer.ReadTag(nameof(Type), s => s.ToEnum<OperateTypes>());
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
    }
}
