using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
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
}
