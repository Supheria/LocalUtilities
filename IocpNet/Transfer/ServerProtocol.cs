using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
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
            if (!CheckLogin(command.Type)) //检测登录
                throw new IocpException(ProtocolCode.NotLogined);
            if (!Commands.TryGetValue(command.Type, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(command);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sendArgs = command.GetOperateSendArgs();
            var callbackArgs = new OperateCallbackArgs(sendArgs)
                .AppendFailure(ex);
            SendCommand(CommandTypes.TransferFile, callbackArgs);
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
        var sendArgs = new OperateSendArgs();
        try
        {
            sendArgs = command.GetOperateSendArgs();
            var name = sendArgs.GetArgs(ProtocolKey.UserName);
            var password = sendArgs.GetArgs(ProtocolKey.Password);
            var type = sendArgs.GetArgs(ProtocolKey.ProtocolType).ToEnum<ProtocolTypes>();
            if (type is ProtocolTypes.None || Type is not ProtocolTypes.None && Type != type)
                throw new IocpException(ProtocolCode.WrongProtocolType);
            Type = type;
            if (type is not ProtocolTypes.HeartBeats)
                DaemonThread?.Stop();
            // TODO: validate userinfo
            UserInfo = new(name, password);
            IsLogin = true;
            HandleLogined();
            var callbackArgs = new OperateCallbackArgs(sendArgs)
                .AppendSuccess();
            SendCommand(CommandTypes.Login, callbackArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var callbackArgs = new OperateCallbackArgs(sendArgs)
                .AppendFailure(ex);
            SendCommand(CommandTypes.TransferFile, callbackArgs);
        }
    }

    public void DoTransferFile(Command command)
    {
        OperateSendArgs sendArgs = new();
        try
        {
            sendArgs = command.GetOperateSendArgs();
            switch (sendArgs.Type)
            {
                case OperateTypes.UploadRequest:
                    DoUploadRequest(sendArgs);
                    return;
                case OperateTypes.UploadContinue:
                    DoUploadContinue(sendArgs, command.Data);
                    return;
                case OperateTypes.DownloadRequest:
                    DoDownloadRequest(sendArgs);
                    return;
                case OperateTypes.DownloadContinue:
                    DoDownloadContinue(sendArgs);
                    return;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var callbackArgs = new OperateCallbackArgs(sendArgs)
                .AppendFailure(ex);
            SendCommand(CommandTypes.TransferFile, callbackArgs);
        }
    }

    private void DoUploadRequest(OperateSendArgs sendArgs)
    {
        var fileArgs = sendArgs.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
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
        var callbackArgs = new OperateCallbackArgs(sendArgs)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCommand(CommandTypes.TransferFile, callbackArgs);
    }

    private void DoUploadContinue(OperateSendArgs sendArgs, byte[] buffer)
    {
        var fileArgs = sendArgs.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        AutoFile.Write(buffer);
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
        var callbackArgs = new OperateCallbackArgs(sendArgs)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCommand(CommandTypes.TransferFile, callbackArgs);
    }

    private void DoDownloadRequest(OperateSendArgs sendArgs)
    {
        var fileArgs = sendArgs.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
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
        var callbackArgs = new OperateCallbackArgs(sendArgs)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCommand(CommandTypes.TransferFile, callbackArgs);
    }

    private void DoDownloadContinue(OperateSendArgs sendArgs)
    {
        var fileArgs = sendArgs.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
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
        var callbackArgs = new OperateCallbackArgs(sendArgs)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString())
            .AppendSuccess();
        SendCommand(CommandTypes.TransferFile, callbackArgs, data, 0, count);
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
