using LocalUtilities.IocpNet.Common;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public sealed class CommandReceiver : Command
{
    public new string GetArgs(ProtocolKey key)
    {
        return base.GetArgs(key);
    }

    public T GetArgs<T>(ProtocolKey key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(base.GetArgs(key));
    }

    public ProtocolCode GetCallbackCode()
    {
        return GetArgs(ProtocolKey.CallbackCode).ToEnum<ProtocolCode>();
    }

    public string GetErrorMessage()
    {
        return new StringBuilder()
            .Append(SignTable.OpenParenthesis)
            .Append(CommandType)
            .Append(SignTable.Comma)
            .Append(SignTable.Space)
            .Append(OperateType)
            .Append(SignTable.CloseParenthesis)
            .Append(GetArgs(ProtocolKey.ErrorMessage))
            .ToString();
    }
}
