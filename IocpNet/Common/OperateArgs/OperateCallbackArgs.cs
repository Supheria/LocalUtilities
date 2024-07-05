using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateCallbackArgs : OperateArgs
{
    public ProtocolCode CallbackCode { get; private set; }

    public string ErrorMessage { get; private set; }

    public override string LocalName => nameof(OperateCallbackArgs);

    public OperateCallbackArgs(string timeStamp, string args, ProtocolCode callbackCode, string errorMessage = "") : base(timeStamp, args)
    {
        CallbackCode = callbackCode;
        ErrorMessage = errorMessage;
        OnSerialize += OperateCallbackArgs_OnSerialize;
        OnDeserialize += OperateCallbackArgs_OnDeserialize;
    }

    public OperateCallbackArgs() : this("", "", ProtocolCode.None)
    {

    }

    public OperateCallbackArgs(string timeStamp, ProtocolCode callbackCode, string errorMessage = "") : this(timeStamp, "", callbackCode, errorMessage)
    {

    }

    private void OperateCallbackArgs_OnSerialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(CallbackCode), CallbackCode.ToString());
        serializer.WriteTag(nameof(ErrorMessage), ErrorMessage);
    }

    private void OperateCallbackArgs_OnDeserialize(SsDeserializer deserializer)
    {
        CallbackCode = deserializer.ReadTag(nameof(CallbackCode), s => s.ToEnum<ProtocolCode>());
        ErrorMessage = deserializer.ReadTag(nameof(ErrorMessage));
    }
}
