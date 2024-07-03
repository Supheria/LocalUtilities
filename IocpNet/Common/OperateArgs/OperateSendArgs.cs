using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.IocpNet.Common;

public sealed class OperateSendArgs
{
    public IocpEventHandler? OnRetry;

    public event IocpEventHandler? OnWasted;

    public LogHandler? OnLog;

    public OperateTypes Type { get; }

    public string Arg { get; }

    public string TimeStamp { get; } = DateTime.Now.ToString(DateTimeFormat.Data);

    DaemonThread DaemonThread { get; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public OperateSendArgs(OperateTypes type, string arg)
    {
        Type = type;
        Arg = arg;
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    private void Retry()
    {
        if (--RetryTimesMax < 0)
        {
            Waste();
            HandleOperateRetryFailed();
            return;
        }
        RetryTimes++;
        DaemonThread.Stop();
        OnRetry?.Invoke();
        HandleOperateRetry();
        DaemonThread.Start();
    }

    public void Waste()
    {
        DaemonThread.Dispose();
        OnWasted?.Invoke();
    }

    private void HandleOperateRetry()
    {
        var message = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(StringTable.Retry)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(Type)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(RetryTimes)
            .Append(SignTable.Space)
            .Append(StringTable.Times)
            .ToString();
        OnLog?.Invoke(message);
    }

    private void HandleOperateRetryFailed()
    {
        var message = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(StringTable.Retry)
            .Append(SignTable.Space)
            .Append(StringTable.Failed)
            .Append(SignTable.Space)
            .Append(SignTable.CloseBracket)
            .Append(Type)
            .ToString();
        OnLog?.Invoke(message);
    }
}
