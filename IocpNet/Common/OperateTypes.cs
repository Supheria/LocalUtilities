namespace LocalUtilities.IocpNet.Common;

public enum OperateTypes : byte
{
    None,
    Message,
    UserList,
    UploadRequest,
    UploadContinue,
    DownloadRequest,
    DownloadContinue,
}
