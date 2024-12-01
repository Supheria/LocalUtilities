using LocalUtilities.General;
using System.Text;

namespace LocalUtilities.IocpNet;

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
            NetException iocp => iocp.ErrorCode.ToWholeString(),
            _ => "UnknownError"
        };
        var message = new StringBuilder()
            .Append(SignCollection.OpenBracket)
            .Append(errorCode)
            .Append(SignCollection.CloseBracket)
            .Append(SignCollection.Space)
            .Append(ex.Message)
            .ToString();
        obj.HandleLog(message);
    }
}
