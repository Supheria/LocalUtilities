using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public abstract class Host
{
    public event LogHandler? OnLog;

    protected UserInfo? UserInfo { get; set; } = null;

    protected void HandleLog(string log)
    {
        log = new StringBuilder()
            .Append(UserInfo?.Name)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(log)
            .Append(SignTable.Space)
            .Append(SignTable.At)
            .Append(DateTime.Now.ToString(DateTimeFormat.Outlook))
            .ToString();
        OnLog?.Invoke(log);
    }

    protected void HandleException(Exception ex)
    {
        HandleLog(ex.Message);
    }

    protected static int WriteU8Buffer(string str, out byte[] buffer)
    {
        buffer = Encoding.UTF8.GetBytes(str);
        return buffer.Length;
    }

    protected static string ReadU8Buffer(byte[] buffer)
    {
        return Encoding.UTF8.GetString(buffer);
    }

    protected void HandleMessage(CommandReceiver receiver)
    {
        var str = new StringBuilder()
            .Append(receiver.GetArgs(ProtocolKey.SendUser))
            .Append(SignTable.Sub)
            .Append(SignTable.Greater)
            .Append(UserInfo?.Name)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(ReadU8Buffer(receiver.Data))
            .ToString();
        OnLog?.Invoke(str);
    }
}
