using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Transfer;

public class Command : SerializableTagValues<ProtocolKey, string>
{
    public CommandTypes Type { get; private set; }

    public override string LocalName => nameof(Command);

    protected override Func<ProtocolKey, string> WriteTag => x => x.ToString();

    protected override Func<string, List<string>> WriteValue => x => [x];

    protected override Func<string, ProtocolKey> ReadTag => s => s.ToEnum<ProtocolKey>();

    protected override Func<List<string>, string> ReadValue => s => s[0];

    public Command(CommandTypes type)
    {
        Type = type;
        OnSerialize += Command_OnSerialize;
        OnDeserialize += Command_OnDeserialize;
    }

    public Command() : this(CommandTypes.None)
    {

    }

    private void Command_OnSerialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(Type), Type.ToString());
    }

    private void Command_OnDeserialize(SsDeserializer deserializer)
    {
        Type = deserializer.ReadTag(nameof(Type), s => s.ToEnum<CommandTypes>());
    }


    public Command AppendSuccess()
    {
        Map[ProtocolKey.CallbackCode] = ProtocolCode.Success.ToString();
        return this;
    }

    public Command AppendFailure(ProtocolCode errorCode, string errorMessage)
    {
        Map[ProtocolKey.CallbackCode] = errorCode.ToString();
        Map[ProtocolKey.ErrorMessage] = errorMessage;
        return this;
    }

    public Command AppendValue(ProtocolKey commandKey, object? value)
    {
        Map[commandKey] = value?.ToString() ?? "";
        return this;
    }

    public Command AppendOperateArgs(OperateArgs args)
    {

        Map[ProtocolKey.OperateArgs] = args.ToSs();
        return this;
    }

    public OperateCallbackArgs GetValueAsCallbackArgs()
    {
        return new OperateCallbackArgs().ParseSs(Map[ProtocolKey.OperateArgs]);
    }

    public OperateSendArgs GetValueAsSendArgs()
    {
        return new OperateSendArgs().ParseSs(Map[ProtocolKey.OperateArgs]);
    }

    // HACK
    public bool GetValueAsString(ProtocolKey key, out string value)
    {
        value = Map[key].ToString();
        return true;
    }

    // HACK
    public bool GetValueAsInt(ProtocolKey key, out int value)
    {
        return int.TryParse(Map[key].ToString(), out value);
    }

    // HACK
    public bool GetValueAsLong(ProtocolKey key, out long value)
    {
        return long.TryParse(Map[key].ToString(), out value);
    }

    // HACK
    public bool GetValueAsBool(ProtocolKey key, out bool value)
    {
        return bool.TryParse(Map[key].ToString(), out value);
    }
}
