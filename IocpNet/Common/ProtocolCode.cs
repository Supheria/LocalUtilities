namespace LocalUtilities.IocpNet.Common;

public enum ProtocolCode
{
    Success,
    NotExistCommand,
    PacketLengthError,
    PacketFormatError,
    UnknowError,
    CommandNoCompleted,
    ParameterError,
    ParameterInvalid,
    UserOrPasswordError,
    UserHasLogined,
    FileNotExist,
    NotOpenFile,
    FileInUse,
    FileExpired,
    NotSameVersion,
    Disconnection,
    FileInProcess,

    DirNotExist,
    CreateDirError,
    DeleteDirError,
    DeleteFileFailed,
    FileSizeError,
    NotLogined,
    FileAlreadyExist,
    NoConnection,
}
