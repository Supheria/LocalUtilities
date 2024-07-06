namespace LocalUtilities.IocpNet.Common;

public class IocpException(ProtocolCode errorCode, string message) : Exception(message)
{
    public ProtocolCode ErrorCode { get; } = errorCode;

    public IocpException(ProtocolCode errorCode) : this(errorCode, "")
    {

    }
}
