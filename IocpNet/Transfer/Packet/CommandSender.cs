using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public sealed class CommandSender : Command
{
    public IocpEventHandler? OnRetry;

    public IocpEventHandler? OnRetryFaild;

    public event IocpEventHandler? OnWasted;

    DaemonThread? DaemonThread { get; set; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public CommandSender(DateTime timeStamp, bool needWaitingCallback, CommandTypes commandType, OperateTypes operateType, byte[] data, int dataOffset, int dataCount) : base(timeStamp, needWaitingCallback, commandType, operateType, data, dataOffset, dataCount)
    {

    }

    public CommandSender(DateTime timeStamp, bool needWaitingCallback, CommandTypes commandType, OperateTypes operateType) : base(timeStamp, needWaitingCallback, commandType, operateType)
    {

    }

    public void StartWaitingCallback()
    {
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public CommandSender AppendSuccess()
    {
        AppendArgs(ProtocolKey.CallbackCode, ProtocolCode.Success.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, "");
        return this;
    }

    public CommandSender AppendFailure(Exception ex)
    {
        var errorCode = ex switch
        {
            IocpException iocp => iocp.ErrorCode,
            _ => ProtocolCode.UnknowError,
        };
        AppendArgs(ProtocolKey.CallbackCode, errorCode.ToString());
        AppendArgs(ProtocolKey.ErrorMessage, ex.Message);
        return this;
    }

    public new CommandSender AppendArgs(ProtocolKey key, string args)
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
                OnRetryFaild?.Invoke();
                HandleOperateRetryFailed();
                return;
            }
            RetryTimes++;
            DaemonThread?.Stop();
            OnRetry.Invoke();
            HandleOperateRetry();
            DaemonThread?.Start();
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
        DaemonThread?.Dispose();
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
