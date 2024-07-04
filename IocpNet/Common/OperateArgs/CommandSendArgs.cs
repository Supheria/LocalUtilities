using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class CommandSendArgs : ISsSerializable
{
    public IocpEventHandler? OnRetry;

    public event IocpEventHandler? OnWasted;

    public LogHandler? OnLog;

    public OperateTypes OperateType { get; private set; }

    public ProtocolTypes ProtocolType { get; private set; }

    public string Data { get; private set; }

    public string TimeStamp { get; private set; } = DateTime.Now.ToString(DateTimeFormat.Data);

    DaemonThread DaemonThread { get; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public string LocalName => nameof(CommandSendArgs);

    public CommandSendArgs(OperateTypes operateType, ProtocolTypes protocolType, string data)
    {
        OperateType = operateType;
        ProtocolType = protocolType;
        Data = data;
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public CommandSendArgs() : this(OperateTypes.None, ProtocolTypes.None, "")
    {

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
            .Append(OperateType)
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
            .Append(OperateType)
            .ToString();
        OnLog?.Invoke(message);
    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(OperateType), OperateType.ToString());
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
        serializer.WriteTag(nameof(Data), Data);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        OperateType = deserializer.ReadTag(nameof(OperateType), s => s.ToEnum<OperateTypes>());
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
        Data = deserializer.ReadTag(nameof(Data));
    }
}
