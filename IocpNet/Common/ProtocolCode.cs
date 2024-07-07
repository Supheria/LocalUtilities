namespace LocalUtilities.IocpNet.Common;

public enum ProtocolCode
{
    None,
    Success,
    UnknownCommand,
    UnknowError,
    WrongProtocolType,
    FileExpired,
    FileNotExist,
    ProcessingFile,
    EmptyUserInfo,
    NoConnection,
    NotSameVersion,
    SameVersionAlreadyExist,
    NotLogined,
    SocketClosed,
    DataOutLimit,
    ServerNotStartYet,
    ServerHasStarted,
    ArgumentError,
    CannotFindSourceSendCommand,
    CannotAddSendCommand,
    UserNotExist,
}
