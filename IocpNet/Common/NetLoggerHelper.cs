using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public static class NetLoggerHelper
{
    public static void HandleLog<T>(this T obj, string message) where T : INetLogger
    {
        obj.OnLog?.Invoke(obj.GetLog(message));
    }

    public static void HandleException<T>(this T obj, Exception ex) where T : INetLogger
    {
        var errorCode = ex switch
        {
            IocpException iocp => iocp.ErrorCode.ToWholeString(),
            _ => StringTable.Null,
        };
        var message = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(errorCode)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(ex.Message)
            .ToString();
        obj.HandleLog(message);
    }
}
