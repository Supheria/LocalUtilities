namespace LocalUtilities.IocpNet.Common;

public sealed class OperateReceiveArgs(OperateTypes type, string args)
{
    public OperateTypes Type { get; } = type;

    public string Arg { get; } = args;
}
