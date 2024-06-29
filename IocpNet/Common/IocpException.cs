using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Common;

public class IocpException(ProtocolCode errorCode, string message) : Exception(message)
{
    public ProtocolCode ErrorCode { get; } = errorCode;

    public override string Message => new StringBuilder()
        .Append(SignTable.OpenBracket)
        .Append(ErrorCode)
        .Append(SignTable.CloseBracket)
        .Append(base.Message)
        .ToString();

    public IocpException(ProtocolCode errorCode) : this(errorCode, "")
    {

    }
}
