using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class CommandCallbackArgs(string timeStamp, ProtocolCode callbackCode, string errorMessage = "") : ISsSerializable
{
    public string TimeStamp { get; private set; } = timeStamp;

    public ProtocolCode CallbackCode { get; private set; } = callbackCode;

    public string ErrorMessage { get; private set; } = errorMessage;

    public string LocalName => nameof(CommandCallbackArgs);

    public CommandCallbackArgs() : this("", ProtocolCode.None)
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
        serializer.WriteTag(nameof(CallbackCode), CallbackCode.ToString());
        serializer.WriteTag(nameof(ErrorMessage), ErrorMessage);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
        CallbackCode = deserializer.ReadTag(nameof(CallbackCode), s => s.ToEnum<ProtocolCode>());
        ErrorMessage = deserializer.ReadTag(nameof(ErrorMessage));
    }
}
