namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateCallbackArgs(string timeStamp, ProtocolCode callbackCode, string? errorMessage = null)
{
    public string TimeStamp { get; } = timeStamp;

    public ProtocolCode CallbackCode { get; } = callbackCode;

    public string? ErrorMessage { get; } = errorMessage;
}