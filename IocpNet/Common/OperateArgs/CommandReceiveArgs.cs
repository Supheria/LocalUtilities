namespace LocalUtilities.IocpNet.Common.OperateArgs;

public sealed class CommandReceiveArgs(OperateTypes type, string data)
{
    public OperateTypes Type { get; } = type;

    public string Arg { get; } = data;
}
