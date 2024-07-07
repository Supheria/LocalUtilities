using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public sealed class CommandSend : Command
{
    public IocpEventHandler? OnRetry;

    public event IocpEventHandler? OnWasted;

    DaemonThread DaemonThread { get; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public CommandSend(CommandTypes commandType, OperateTypes operateType, byte[] data, int dataOffset, int dataCount) : base(Types.Send, DateTime.Now, commandType, operateType, data, dataOffset, dataCount)
    {
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public CommandSend(CommandTypes commandType, OperateTypes operateType) : base(Types.Send, DateTime.Now, commandType, operateType)
    {
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public override CommandSend AppendArgs(ProtocolKey key, string args)
    {
        base.AppendArgs(key, args);
        return this;
    }

    private void Retry()
    {
        try
        {
            if (OnRetry is null)
            {
                Waste();
                return;
            }
            if (--RetryTimesMax < 0)
            {
                Waste();
                HandleOperateRetryFailed();
                return;
            }
            RetryTimes++;
            DaemonThread.Stop();
            OnRetry.Invoke();
            HandleOperateRetry();
            DaemonThread.Start();
        }
        catch (Exception ex)
        {
            HandleLog(ex.Message);
            Waste();
            HandleOperateRetryFailed();
        }
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
            .Append(OperateType)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(RetryTimes)
            .Append(SignTable.Space)
            .Append(StringTable.Times)
            .ToString();
        HandleLog(message);
    }

    private void HandleOperateRetryFailed()
    {
        var message = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(StringTable.Retry)
            .Append(SignTable.Space)
            .Append(StringTable.Failed)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(OperateType)
            .ToString();
        HandleLog(message);
    }
}
