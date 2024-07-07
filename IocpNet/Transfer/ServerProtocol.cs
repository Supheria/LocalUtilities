using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class ServerProtocol : Protocol
{
    public ProtocolTypes Type { get; private set; } = ProtocolTypes.None;

    protected override string RepoPath { get; set; } = @"repo\server";

    public string TimeStamp { get; } = DateTime.Now.ToString(DateTimeFormat.Data);

    public ServerProtocol()
    {
        DaemonThread = new(ConstTabel.SocketTimeoutMilliseconds, CheckTimeout);
        Commands[CommandTypes.Login] = DoLogin;
        Commands[CommandTypes.HeartBeats] = (_) => { };
        Commands[CommandTypes.TransferFile] = DoTransferFile;
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

    protected override void ProcessCommand(Command command)
    {
        try
        {
            if (!CheckLogin(command.CommandType)) //检测登录
                throw new IocpException(ProtocolCode.NotLogined);
            if (!Commands.TryGetValue(command.CommandType, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(command);
        }
        catch (Exception ex)
        {
            HandleException(nameof(ProcessCommand), ex);
        }
    }

    private bool CheckLogin(CommandTypes type)
    {
        if (type is CommandTypes.Login)
            return true;
        else
            return IsLogin;
    }

    private void DoLogin(Command command)
    {
        var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType);
        try
        {
            var name = command.GetArgs(ProtocolKey.UserName);
            var password = command.GetArgs(ProtocolKey.Password);
            var type = command.GetArgs(ProtocolKey.ProtocolType).ToEnum<ProtocolTypes>();
            if (type is ProtocolTypes.None || Type is not ProtocolTypes.None && Type != type)
                throw new IocpException(ProtocolCode.WrongProtocolType);
            Type = type;
            if (type is not ProtocolTypes.HeartBeats)
                DaemonThread?.Stop();
            // TODO: validate userinfo
            UserInfo = new(name, password);
            IsLogin = true;
            HandleLogined();
            commandCallback.AppendSuccess();
        }
        catch (Exception ex)
        {
            HandleException(nameof(DoLogin), ex);
            commandCallback.AppendFailure(ex);
        }
        SendCallback(commandCallback);
    }

    public void DoTransferFile(Command command)
    {
        try
        {
            switch (command.OperateType)
            {
                case OperateTypes.UploadRequest:
                    DoUploadRequest(command);
                    return;
                case OperateTypes.UploadContinue:
                    DoUploadContinue(command);
                    return;
                case OperateTypes.DownloadRequest:
                    DoDownloadRequest(command);
                    return;
                case OperateTypes.DownloadContinue:
                    DoDownloadContinue(command);
                    return;
            }
        }
        catch (Exception ex)
        {
            HandleException(nameof(DoTransferFile), ex);
            var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType)
                .AppendFailure(ex);
            SendCallback(commandCallback);
        }
    }

    private void DoUploadRequest(Command command)
    {
        var fileArgs = command.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        if (File.Exists(filePath))
        {
            var fileTest = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (fileTest.ToMd5HashString() == fileArgs.Md5Value)
            {
                fileTest.Dispose();
                throw new IocpException(ProtocolCode.SameVersionAlreadyExist);
            }
            fileTest.Dispose();
            File.Delete(filePath);
        }
        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        HandleUploadStart();
        var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCallback(commandCallback);
    }

    private void DoUploadContinue(Command command)
    {
        var fileArgs = command.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        AutoFile.Write(command.Data);
        // simple validation
        if (AutoFile.Position != fileArgs.FilePosition)
            throw new IocpException(ProtocolCode.NotSameVersion);
        if (AutoFile.Position >= fileArgs.FileLength)
        {
            AutoFile.Dispose();
            HandleUploaded(fileArgs.StartTime);
        }
        else
            HandleUploading(fileArgs.FileLength, AutoFile.Position);
        var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCallback(commandCallback);
    }

    private void DoDownloadRequest(Command command)
    {
        var fileArgs = command.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        if (!File.Exists(filePath))
            throw new IocpException(ProtocolCode.FileNotExist, filePath);

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (fileStream.ToMd5HashString() == fileArgs.Md5Value)
        {
            fileStream.Dispose();
            throw new IocpException(ProtocolCode.SameVersionAlreadyExist);
        }
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        HandleDownloadStart();
        fileArgs.FileLength = AutoFile.Length;
        fileArgs.PacketLength = AutoFile.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : AutoFile.Length;
        var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCallback(commandCallback);
    }

    private void DoDownloadContinue(Command command)
    {
        var fileArgs = command.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        AutoFile.Position = fileArgs.FilePosition;
        if (AutoFile.Position >= AutoFile.Length)
        {
            AutoFile.Dispose();
            HandleDownloaded(fileArgs.StartTime);
            return;
        }
        var data = new byte[fileArgs.PacketLength];
        if (!AutoFile.Read(data, out var count))
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        HandleDownloading(AutoFile.Length, AutoFile.Position);
        fileArgs.FilePosition = AutoFile.Position;
        var commandCallback = new CommandCallback(command.TimeStamp, command.CommandType, command.OperateType, data, 0, count)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCallback(commandCallback);
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
