using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public sealed class OperateCallbackArgs(string timeStamp, ProtocolCode callbackCode, string? errorMessage = null)
{
    public string TimeStamp { get; } = timeStamp;

    public ProtocolCode CallbackCode { get; } = callbackCode;

    public string? ErrorMessage { get; } = errorMessage;
}