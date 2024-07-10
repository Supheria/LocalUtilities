using LocalUtilities.IocpNet.Transfer;

namespace LocalUtilities.IocpNet.Common;

public class IocpException(Enum errorCode, string message) : Exception(message)
{
    public Enum ErrorCode { get; } = errorCode;

    public IocpException(Enum errorCode) : this(errorCode, "")
    {

    }
}
