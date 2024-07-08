namespace LocalUtilities.IocpNet.Common;

public enum OperateTypes : byte
{
    None,
    Message,
    UpdateUserList,
    UploadRequest,
    UploadContinue,
    UploadFinish,
    DownloadRequest,
    DownloadContinue,
    DownloadFinish,
}
