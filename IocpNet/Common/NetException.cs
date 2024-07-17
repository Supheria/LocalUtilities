using LocalUtilities.TypeToolKit.Convert;

namespace LocalUtilities.IocpNet.Common;

public class NetException(Enum errorCode, string? message) : Exception(message)
{
    public Enum ErrorCode { get; } = errorCode;

    public NetException(Enum errorCode) : this(errorCode, "")
    {

    }

    public NetException(Enum errorCode, params string[] keys) : this(errorCode, keys.ToArrayString())
    {

    }
}
