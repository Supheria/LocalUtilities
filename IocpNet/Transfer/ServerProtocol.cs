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
        Commands[CommandTypes.HeartBeats] = DoHeartBeats;
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

    protected override void ProcessCommand(CommandReceiver receiverr)
    {
        try
        {
            if (!CheckLogin(receiverr.CommandType)) //检测登录
                throw new IocpException(ProtocolCode.NotLogined);
            if (!Commands.TryGetValue(receiverr.CommandType, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(receiverr);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private bool CheckLogin(CommandTypes type)
    {
        if (type is CommandTypes.Login)
            return true;
        else
            return IsLogin;
    }

    private void DoHeartBeats(CommandReceiver receiver)
    {
        var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
        CallbackSuccess(sender);
    }

    private void DoLogin(CommandReceiver receiver)
    {
        var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
        try
        {
            var name = receiver.GetArgs(ProtocolKey.UserName);
            var password = receiver.GetArgs(ProtocolKey.Password);
            var type = receiver.GetArgs(ProtocolKey.ProtocolType).ToEnum<ProtocolTypes>();
            if (type is ProtocolTypes.None || Type is not ProtocolTypes.None && Type != type)
                throw new IocpException(ProtocolCode.WrongProtocolType);
            Type = type;
            if (type is not ProtocolTypes.HeartBeats)
                DaemonThread?.Stop();
            // TODO: validate userinfo
            UserInfo = new(name, password);
            IsLogin = true;
            HandleLogined();
            CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            CallbackFailure(sender, ex);
        }
    }

    public void DoTransferFile(CommandReceiver receiver)
    {
        switch (receiver.OperateType)
        {
            case OperateTypes.UploadRequest:
                DoUploadRequestAsync(receiver);
                break;
            case OperateTypes.UploadContinue:
                DoUploadContinue(receiver);
                break;
            case OperateTypes.DownloadRequest:
                DoDownloadRequestAsync(receiver);
                break;
            case OperateTypes.DownloadContinue:
                DoDownloadContinue(receiver);
                break;
            case OperateTypes.DownloadFinish:
                DoDownloadFinish(receiver);
                break;
        }
    }

    private async void DoUploadRequestAsync(CommandReceiver receiver)
    {
        try
        {
            var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
            var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
            if (File.Exists(filePath))
            {
                var task = Task.Run(() =>
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return fileStream.ToMd5HashString();
                });
                if (await task == fileArgs.Md5Value)
                    throw new IocpException(ProtocolCode.SameVersionAlreadyExist);
                File.Delete(filePath);
            }
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            if (!AutoFile.Relocate(fileStream))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleUploadStart();
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackFailure(sender, ex);
        }
    }

    private void DoUploadContinue(CommandReceiver receiver)
    {
        try
        {
            var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
            AutoFile.Write(receiver.Data);
            // simple validation
            if (AutoFile.Position != fileArgs.FilePosition)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (AutoFile.Position < fileArgs.FileLength)
            {
                HandleUploading(fileArgs.FileLength, AutoFile.Position);
                var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType)
                    .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
                CallbackSuccess(sender);
            }
            else
            {
                AutoFile.Dispose();
                HandleUploaded(fileArgs.StartTime);
                var startTime = BitConverter.GetBytes(fileArgs.StartTime.ToBinary());
                var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, OperateTypes.UploadFinish, startTime, 0, startTime.Length);
                CallbackSuccess(sender);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackFailure(sender, ex);
        }
    }

    private async void DoDownloadRequestAsync(CommandReceiver receiver)
    {
        try
        {
            var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
            var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var task = Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return fileStream.ToMd5HashString();
            });
            if (await task == fileArgs.Md5Value)
                throw new IocpException(ProtocolCode.SameVersionAlreadyExist);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!AutoFile.Relocate(fileStream))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleDownloadStart();
            fileArgs.FileLength = AutoFile.Length;
            fileArgs.PacketLength = AutoFile.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : AutoFile.Length;
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackFailure(sender, ex);
        }
    }

    private void DoDownloadContinue(CommandReceiver receiver)
    {
        try
        {
            var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
            AutoFile.Position = fileArgs.FilePosition;
            var data = new byte[fileArgs.PacketLength];
            if (!AutoFile.Read(data, out var count))
                throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
            HandleDownloading(AutoFile.Length, AutoFile.Position);
            fileArgs.FilePosition = AutoFile.Position;
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType, data, 0, count)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackFailure(sender, ex);
        }
    }

    private void DoDownloadFinish(CommandReceiver receiver)
    {
        try
        {
            AutoFile.Dispose();
            var startTime = DateTime.FromBinary(BitConverter.ToInt64(receiver.Data));
            HandleDownloaded(startTime);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            var sender = new CommandSender(receiver.TimeStamp, receiver.CommandType, receiver.OperateType);
            CallbackFailure(sender, ex);
        }
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
