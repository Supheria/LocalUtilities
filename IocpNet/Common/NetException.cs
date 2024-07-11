namespace LocalUtilities.IocpNet.Common;

public class NetException(Enum errorCode, string message) : Exception(message)
{
    public Enum ErrorCode { get; } = errorCode;

    public NetException(Enum errorCode) : this(errorCode, "")
    {

    }
}
