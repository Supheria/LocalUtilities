using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public class CommandArgs : SerializableTagValues<ProtocolKey, string>
{
    protected override Func<ProtocolKey, string> WriteTag => x => x.ToString();

    protected override Func<string, List<string>> WriteValue => x => [x];

    protected override Func<string, ProtocolKey> ReadTag => s => s.ToEnum<ProtocolKey>();

    protected override Func<List<string>, string> ReadValue => s => s[0];

    public override string LocalName => nameof(CommandArgs);
}
