using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateSendArgs : OperateArgs
{
    public IocpEventHandler? OnRetry;

    public event IocpEventHandler? OnWasted;

    public LogHandler? OnLog;

    DaemonThread DaemonThread { get; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public override string LocalName => nameof(OperateSendArgs);

    private OperateSendArgs(OperateTypes type, string timeStamp) : base(type, timeStamp)
    {
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public OperateSendArgs(OperateTypes type) : this(type, DateTime.Now.ToString(DateTimeFormat.Data))
    {

    }

    public OperateSendArgs() : this(OperateTypes.None, "")
    {

    }

    private void Retry()
    {
        try
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
        catch (Exception ex)
        {
            OnLog?.Invoke(ex.Message);
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
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(Type)
            .ToString();
        OnLog?.Invoke(message);
    }

    public OperateSendArgs AppendArgs(ProtocolKey key, string args)
    {
        Map[key] = args;
        return this;
    }

    public string GetArgs(ProtocolKey key)
    {
        return Map[key];
    }

    public T GetArgs<T>(ProtocolKey key) where T : ISsSerializable, new()
    {
        return new T().ParseSs(Map[key]);
    }
}
