namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class OperateReceiveArgs(OperateTypes type, string arg)
{
    public OperateTypes Type { get; } = type;

    public string Arg { get; } = arg;
}
