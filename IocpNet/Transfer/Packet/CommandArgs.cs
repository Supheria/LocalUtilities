using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public class CommandArgs : SerializableTagValues<string, string>
{
    protected override Func<string, string> WriteTag => x => x.ToString();

    protected override Func<string, List<string>> WriteValue => x => [x];

    protected override Func<string, string> ReadTag => s => s;

    protected override Func<List<string>, string> ReadValue => s => s[0];

    public override string LocalName => nameof(CommandArgs);
}
