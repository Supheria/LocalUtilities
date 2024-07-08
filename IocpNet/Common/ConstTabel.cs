namespace LocalUtilities.IocpNet.Common;

public class ConstTabel
{
    /// <summary>
    /// 解析命令初始缓存大小，设置过小会造成事件响应增多，设置过大会造成内存占用偏多
    /// </summary>
    public const int InitialBufferSize = 1024 * 4;

    public const int DataBytesTransferredMax = 1024 * 1024;

    public const int SocketTimeoutMilliseconds = 20 * 1000;

    public const int HeartBeatsInterval = 10 * 1000;

    public const int FileStreamExpireMilliseconds = 5 * 1000;

    public const int WaitingCallbackMilliseconds = 3 * 1000;

    public const int OperateRetryTimes = 3;

    public const int OneKB = 1024;

    public const int OneMB = 1024 * 1024;
}
