namespace LocalUtilities.IocpNet.Common;

public enum CommandTypes : byte
{
    None,
    Login,
    Operate,
    OperateCallback,
    HeartBeats,
    TransferFile
}
