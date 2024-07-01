using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common;

public sealed class OperateSendArgs
{
    public IocpEventHandler? OnWaste;

    public OperateTypes Type { get; }

    public string Args { get; }

    public string TimeStamp { get; } = DateTime.Now.ToString(DateTimeFormat.Data);

    DaemonThread DaemonThread { get; }

    int RetryTimes { get; set; } = ConstTabel.OperateArgsRetryTimes;

    public OperateSendArgs(OperateTypes type, string args)
    {
        Type = type;
        Args = args;
        DaemonThread = new(ConstTabel.OperateArgsWasteMilliseconds, Waste);
        DaemonThread.Start();
    }

    public void Waste()
    {
        OnWaste?.Invoke();
        DaemonThread.Dispose();
    }

    public void Reuse()
    {
        if (--RetryTimes < 0)
        {
            Waste();
            return;
        }
        DaemonThread.Stop();
        DaemonThread.Start();
    }
}
