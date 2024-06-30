namespace LocalUtilities.IocpNet.Common;

public enum ProtocolCode
{
    Success,
    UnknownCommand,
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
    ProcessingFile,

    DirNotExist,
    CreateDirError,
    DeleteDirError,
    DeleteFileFailed,
    FileSizeError,
    NotLogined,
    FileAlreadyExist,
    NoConnection,
    EmptyUserInfo
}
