﻿using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateSendArgs : OperateArgs
{
    public IocpEventHandler? OnRetry;

    public event IocpEventHandler? OnWasted;

    public LogHandler? OnLog;

    public OperateTypes Type { get; private set; }

    DaemonThread DaemonThread { get; }

    int RetryTimesMax { get; set; } = ConstTabel.OperateRetryTimes;

    int RetryTimes { get; set; } = 0;

    public override string LocalName => nameof(OperateSendArgs);

    public OperateSendArgs(OperateTypes type, string data) : base(DateTime.Now.ToString(DateTimeFormat.Data), data)
    {
        Data = data;
        Type = type;
        DaemonThread = new(ConstTabel.OperateRetryInterval, Retry);
        DaemonThread.Start();
    }

    public OperateSendArgs() : this(OperateTypes.None, "")
    {

    }

    public override void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(Type), Type.ToString());
        serializer.WriteTag(nameof(TimeStamp), TimeStamp);
        serializer.WriteTag(nameof(Data), Data);
    }

    public override void Deserialize(SsDeserializer deserializer)
    {
        Type = deserializer.ReadTag(nameof(Type), s => s.ToEnum<OperateTypes>());
        TimeStamp = deserializer.ReadTag(nameof(TimeStamp));
        Data = deserializer.ReadTag(nameof(Data));
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
            .Append(SignTable.Space)
            .Append(SignTable.CloseBracket)
            .Append(Type)
            .ToString();
        OnLog?.Invoke(message);
    }
}