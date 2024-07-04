namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateReceiveArgs(OperateTypes type, string data)
{
    public OperateTypes Type { get; } = type;

    public string Arg { get; } = data;
}
