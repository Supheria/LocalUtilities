using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class ServerProtocol : Protocol
{
    public ProtocolTypes Type { get; private set; } = ProtocolTypes.None;

    public string TimeStamp { get; } = DateTime.Now.ToString(DateTimeFormat.Data);

    public ServerProtocol()
    {
        DaemonThread = new(ConstTabel.SocketTimeoutMilliseconds, CheckTimeout);
        Commands[CommandTypes.Login] = DoLogin;
        Commands[CommandTypes.HeartBeats] = (_, _, _, _) => { };
        Commands[CommandTypes.Upload] = DoUpload;
        Commands[CommandTypes.WriteFile] = DoWriteFile;
        Commands[CommandTypes.Download] = DoDownload;
    }

    private void CheckTimeout()
    {
        var span = DateTime.Now - SocketInfo.ActiveTime;
        if (span.TotalMilliseconds < ConstTabel.SocketTimeoutMilliseconds)
            return;
        Close();
    }

    public void ProcessAccept(Socket acceptSocket)
    {
        if (Socket is not null)
            return;
        Socket = acceptSocket;
        SocketInfo.Connect(acceptSocket);
        DaemonThread?.Start();
        ReceiveAsync();
    }

    private void CommandFail(Exception ex)
    {
        CommandFail(ex, new());
    }

    private void CommandFail(Exception ex, Command command)
    {
        if (ex is IocpException iocp)
            command.AppendFailure(iocp.ErrorCode, iocp.Message);
        else
            command.AppendFailure(ProtocolCode.UnknowError, ex.Message);
        WriteCommand(command);
        SendAsync();
        HandleException(ex);
    }

    private void CommandSucceed(Command command)
    {
        CommandSucceed(command, [], 0, 0);
    }

    private void CommandSucceed(Command command, byte[] buffer, int offset, int count)
    {
        command.AppendSuccess();
        WriteCommand(command, buffer, offset, count);
        SendAsync();
    }

    protected override void ProcessCommand(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!CheckLogin(command.Type)) //检测登录
                throw new IocpException(ProtocolCode.NotLogined);
            if (!Commands.TryGetValue(command.Type, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(command, buffer, offset, count);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    private bool CheckLogin(CommandTypes type)
    {
        if (type is CommandTypes.Login)
            return true;
        else
            return IsLogin;
    }

    private void DoLogin(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!command.GetValueAsString(ProtocolKey.UserName, out var name) ||
                !command.GetValueAsString(ProtocolKey.Password, out var password) ||
                !command.GetValueAsString(ProtocolKey.ProtocolType, out var t))
                throw new IocpException(ProtocolCode.ParameterError);
            var type = t.ToEnum<ProtocolTypes>();
            if (type is ProtocolTypes.None || Type is not ProtocolTypes.None && Type != type)
                throw new IocpException(ProtocolCode.WrongProtocolType);
            Type = type;
            if (type is not ProtocolTypes.HeartBeats)
                DaemonThread?.Stop();
            // TODO: validate userinfo
            UserInfo = new(name, password);
            IsLogin = true;
            HandleLogined();
            command = new Command(CommandTypes.Login);
            CommandSucceed(command);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    public void DoUpload(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!command.GetValueAsString(ProtocolKey.DirName, out var dirName) ||
                !command.GetValueAsString(ProtocolKey.FileName, out var fileName) ||
                !command.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !command.GetValueAsLong(ProtocolKey.PacketLength, out var packetLength) ||
                !command.GetValueAsBool(ProtocolKey.CanRename, out var canRename))
                throw new IocpException(ProtocolCode.ParameterError, "");
            var filePath = GetFileRepoPath(dirName, fileName);
            if (File.Exists(filePath))
            {
                if (!canRename)
                    throw new IocpException(ProtocolCode.FileAlreadyExist, filePath);
                filePath = filePath.RenamePathByDateTime();
            }
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleUploadStart();
            command = new Command(CommandTypes.Upload)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.Position, 0);
            CommandSucceed(command);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }

    }

    private void DoWriteFile(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!command.GetValueAsLong(ProtocolKey.FileLength, out var fileLength) ||
                !command.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !command.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength) ||
                !command.GetValueAsLong(ProtocolKey.Position, out var position))
                throw new IocpException(ProtocolCode.ParameterError);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            AutoFile.Write(buffer, offset, count);
            // simple validation
            if (AutoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (AutoFile.Length >= fileLength)
            {
                // TODO: log success
                AutoFile.DisposeFileStream();
                HandleUploaded(startTime);
            }
            else
                HandleUploading(fileLength, AutoFile.Position);
            command = new Command(CommandTypes.Upload)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength);
            CommandSucceed(command);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    private void DoDownload(Command command, byte[] buffer, int offset, int count)
    {
        OperateSendArgs sendArgs = new();
        try
        {
            sendArgs = command.GetValueAsSendArgs();
            var downloadArgs = new DownloadArgs().ParseSs(sendArgs.Data);
            switch (sendArgs.Type)
            {
                case OperateTypes.DownloadRequest:
                    DoDownloadRequest(downloadArgs);
                    break;
                case OperateTypes.DownloadContinue:
                    DoDownloadContinue(downloadArgs, out buffer, out count);
                    break;
            }
            var callbackArgs = new OperateCallbackArgs(sendArgs.TimeStamp, downloadArgs.ToSs(), ProtocolCode.Success);
            SendCommand(CommandTypes.Download, callbackArgs, buffer, 0, count);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var errorCode = ex switch
            {
                IocpException iocp => iocp.ErrorCode,
                _ => ProtocolCode.UnknowError,
            };
            var callbackArgs = new OperateCallbackArgs(sendArgs.TimeStamp, errorCode, ex.Message);
            SendCommand(CommandTypes.Download, callbackArgs);
        }
    }

    private void DoDownloadRequest(DownloadArgs args)
    {
        var filePath = GetFileRepoPath(args.DirName, args.FileName);
        if (!File.Exists(filePath))
            throw new IocpException(ProtocolCode.FileNotExist, filePath);
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
            throw new IocpException(ProtocolCode.ProcessingFile);
        HandleDownloadStart();
        args.FileLength = AutoFile.Length;
        args.PacketLength = fileStream.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : fileStream.Length;
    }

    private void DoDownloadContinue(DownloadArgs args, out byte[] fileData, out int count)
    {
        fileData = [];
        count = 0;
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, args.FileName);
        AutoFile.Position = args.FilePosition;
        if (AutoFile.Position >= AutoFile.Length)
        {
            AutoFile.DisposeFileStream();
            HandleDownloaded(args.StartTime);
            return;
        }
        fileData = new byte[args.PacketLength];
        if (!AutoFile.Read(fileData, 0, fileData.Length, out count))
            throw new IocpException(ProtocolCode.FileExpired, args.FileName);
        HandleDownloading(AutoFile.Length, AutoFile.Position);
        args.FilePosition = AutoFile.Position;
    }

    protected override string GetLog(string log)
    {
        return new StringBuilder()
                .Append(SignTable.OpenBracket)
                .Append(SocketInfo.RemoteEndPoint)
                .Append(SignTable.CloseBracket)
                .Append(SignTable.Space)
                .Append(log)
                .ToString();
    }

    //private void DoDir(CommandParser commandParser)
    //{
    //    if (!commandParser.GetValueAsString(ProtocolKey.ParentDir, out var dir))
    //    {
    //        CommandFail(ProtocolCode.ParameterError, "");
    //        return;
    //    }
    //    if (!Directory.Exists(dir))
    //    {
    //        CommandFail(ProtocolCode.DirNotExist, dir);
    //        return;
    //    }
    //    char[] directorySeparator = [Path.DirectorySeparatorChar];
    //    try
    //    {
    //        var commandComposer = new CommandComposer()
    //            .AppendCommand(ProtocolKey.Dir);
    //        foreach (var subDir in Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly))
    //        {
    //            var dirName = subDir.Split(directorySeparator, StringSplitOptions.RemoveEmptyEntries);
    //            commandComposer.AppendValue(ProtocolKey.Item, dirName[dirName.Length - 1]);

    //        }
    //        CommandSucceed(commandComposer);
    //    }
    //    catch (Exception ex)
    //    {
    //        CommandFail(ProtocolCode.UnknowError, ex.Message);
    //    }
    //}

    //private void DoFileList(CommandParser commandParser)
    //{
    //    if (!commandParser.GetValueAsString(ProtocolKey.DirName, out var dir))
    //    {
    //        CommandFail(ProtocolCode.ParameterError, "");
    //        return;
    //    }
    //    dir = dir is "" ? RootDirectoryPath : Path.Combine(RootDirectoryPath, dir);
    //    if (!Directory.Exists(dir))
    //    {
    //        CommandFail(ProtocolCode.DirNotExist, dir);
    //        return;
    //    }
    //    try
    //    {
    //        var commandComposer = new CommandComposer()
    //            .AppendCommand(ProtocolKey.FileList);
    //        foreach (var file in Directory.GetFiles(dir))
    //        {
    //            var fileInfo = new FileInfo(file);
    //            commandComposer.AppendValue(ProtocolKey.Item, fileInfo.Name + ProtocolKey.TextSeperator + fileInfo.Length.ToString());
    //        }
    //        CommandSucceed(commandComposer);
    //    }
    //    catch (Exception ex)
    //    {
    //        CommandFail(ProtocolCode.UnknowError, ex.Message);
    //    }
    //}
}
