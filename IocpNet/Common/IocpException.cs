using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Common;

public class IocpException(ProtocolCode errorCode, string message) : Exception(message)
{
    public ProtocolCode ErrorCode { get; } = errorCode;

    public IocpException(ProtocolCode errorCode) : this(errorCode, "")
    {

    }

    public static IocpException ArgumentNull(string name)
    {
        return new(ProtocolCode.ArgumentNull, name);
    }
}
