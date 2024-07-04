using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateCallbackArgs(string timeStamp, string data, ProtocolCode callbackCode, string errorMessage = "") : ISsSerializable
{
    public string TimeStamp { get; private set; } = timeStamp;

    public string Data { get; private set; } = data;

    public ProtocolCode CallbackCode { get; private set; } = callbackCode;

    public string ErrorMessage { get; private set; } = errorMessage;

    public string LocalName => nameof(OperateCallbackArgs);

    public OperateCallbackArgs() : this("", "", ProtocolCode.None)
    {

    }

    public OperateCallbackArgs(string timeStamp, ProtocolCode callbackCode, string errorMessage = "") : this(timeStamp, "", callbackCode, errorMessage)
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
        serializer.WriteTag(nameof(Data), Data);
        serializer.WriteTag(nameof(CallbackCode), CallbackCode.ToString());
        serializer.WriteTag(nameof(ErrorMessage), ErrorMessage);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
        Data = deserializer.ReadTag(nameof(Data));
        CallbackCode = deserializer.ReadTag(nameof(CallbackCode), s => s.ToEnum<ProtocolCode>());
        ErrorMessage = deserializer.ReadTag(nameof(ErrorMessage));
    }
}
