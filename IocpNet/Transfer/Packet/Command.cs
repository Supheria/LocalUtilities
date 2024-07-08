using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.IocpNet.Transfer.Packet;

public abstract class Command
{
    public event LogHandler? OnLog;

    public DateTime TimeStamp { get; protected init; } = new();

    public CommandTypes CommandType { get; protected init; } = CommandTypes.None;

    public OperateTypes OperateType { get; protected init; } = OperateTypes.None;

    protected CommandArgs Args { get; init; } = new();

    public byte[] Data { get; protected init; } = [];

    protected void HandleLog(string message)
    {
        OnLog?.Invoke(message);
    }

    protected const int HeadLength
        = sizeof(int) // total length
        + sizeof(int) // args length
        + sizeof(byte) // command type
        + sizeof(byte) // operate type
        + sizeof(long) // time stamp
        ;
}
