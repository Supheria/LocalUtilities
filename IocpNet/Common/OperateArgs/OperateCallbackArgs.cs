using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateCallbackArgs : OperateArgs
{
    public override string LocalName => nameof(OperateCallbackArgs);

    private OperateCallbackArgs(OperateTypes type, string timeStamp) : base(type, timeStamp)
    {

    }

    public OperateCallbackArgs(OperateSendArgs sendArgs) : this(sendArgs.Type, sendArgs.TimeStamp)
    {

    }

    public OperateCallbackArgs() : this(OperateTypes.None, "")
    {

    }

    public OperateCallbackArgs AppendSuccess()
    {
        Map[ProtocolKey.CallbackCode] = ProtocolCode.Success.ToString();
        Map[ProtocolKey.ErrorMessage] = "";
        return this;
    }

    public OperateCallbackArgs AppendFailure(Exception ex)
    {
        var errorCode = ex switch
        {
            IocpException iocp => iocp.ErrorCode,
            _ => ProtocolCode.UnknowError,
        };
        Map[ProtocolKey.CallbackCode] = errorCode.ToString();
        Map[ProtocolKey.ErrorMessage] = ex.Message;
        return this;
    }

    public ProtocolCode GetCallbackCode()
    {
        return Map[ProtocolKey.CallbackCode].ToEnum<ProtocolCode>();
    }

    public string GetErrorMessage()
    {
        return Map[ProtocolKey.ErrorMessage];
    }

    public OperateCallbackArgs AppendArgs(ProtocolKey key, string args)
    {
        Map[key] = args;
        return this;
    }

    public string GetArgs(ProtocolKey key)
    {
        return Map[key];
    }

    public T GetArgs<T>(ProtocolKey key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(Map[key]);
    }
}
