using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class ClientProtocol : Protocol
{
    public ProtocolTypes Type { get; }

    protected override string RepoPath { get; set; } = @"repo\client";

    AutoResetEvent ConnectDone { get; } = new(false);

    AutoResetEvent LoginDone { get; } = new(false);

    static int ResetSpan { get; } = 1000;

    bool IsConnect { get; set; } = false;

    public ClientProtocol(ProtocolTypes type)
    {
        Type = type;
        if (type is ProtocolTypes.HeartBeats)
            DaemonThread = new(ConstTabel.HeartBeatsInterval, HeartBeats);
        Commands[CommandTypes.Login] = DoLogin;
        Commands[CommandTypes.TransferFile] = DoTransferFile;
    }

    private void HeartBeats()
    {
        try
        {
            var sender = new CommandSender(DateTime.Now, false, CommandTypes.HeartBeats, OperateTypes.None);
            SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(nameof(HeartBeats), ex);
        }
    }

    public void Connect(IPEndPoint? host, UserInfo? userInfo)
    {
        try
        {
            if (userInfo is null)
                throw new IocpException(ProtocolCode.EmptyUserInfo);
            if (IsLogin && SocketInfo.RemoteEndPoint?.ToString() == host?.ToString())
                return;
            connect();
            if (!IsConnect)
                throw new IocpException(ProtocolCode.NoConnection);
            UserInfo = userInfo;
            var sender = new CommandSender(DateTime.Now, false, CommandTypes.Login, OperateTypes.None)
                .AppendArgs(ProtocolKey.UserName, userInfo.Name ?? "")
                .AppendArgs(ProtocolKey.Password, userInfo.Password)
                .AppendArgs(ProtocolKey.ProtocolType, Type.ToString());
            SendCommand(sender);
            LoginDone?.WaitOne(ResetSpan);
            DaemonThread?.Start();
        }
        catch (Exception ex)
        {
            Close();
            HandleException(nameof(Connect), ex);
            // TODO: log fail
        }
        void connect()
        {
            Close();
            IsConnect = false;
            var connectArgs = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = host,
            };
            connectArgs.Completed += (_, args) => ProcessConnect(args);
            Socket ??= new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!Socket.ConnectAsync(connectArgs))
                ProcessConnect(connectArgs);
            ConnectDone.WaitOne(ResetSpan);
        }
    }

    private void ProcessConnect(SocketAsyncEventArgs connectArgs)
    {
        if (connectArgs.ConnectSocket is null)
        {
            Socket?.Close();
            Socket?.Dispose();
            return;
        }
        ReceiveAsync();
        SocketInfo.Connect(connectArgs.ConnectSocket);
        IsConnect = true;
        ConnectDone.Set();
    }

    protected override void ProcessCommand(CommandReceiver reciver)
    {
        try
        {
            //if (!commandParser.GetValueAsString(ProtocolKey.Code, out var errorCode))
            //    throw new IocpException(ProtocolCode.UnknowError);
            //var code = errorCode.ToEnum<ProtocolCode>();
            //if (code is not ProtocolCode.Success)
            //{
            //    if (commandParser.GetValueAsString(ProtocolKey.Message, out var message))
            //        throw new IocpException(code, message);
            //    else
            //        throw new IocpException(code);
            //}
            if (!Commands.TryGetValue(reciver.CommandType, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(reciver);
        }
        catch (Exception ex)
        {
            HandleException(nameof(ProcessCommand), ex);
            // TODO: log fail
        }
    }

    private void DoLogin(CommandReceiver receiver)
    {
        try
        {
            ReceiveCallback(receiver);
            IsLogin = true;
            LoginDone.Set();
            HandleLogined();
        }
        catch (Exception ex)
        {
            HandleException(nameof(HeartBeats), ex);
        }
    }

    //public void Upload(string dirName, string fileName, bool canRename)
    //{
    //    try
    //    {
    //        if (!AutoFile.IsExpired)
    //            throw new IocpException(ProtocolCode.ProcessingFile);
    //        var filePath = GetFileRepoPath(dirName, fileName);
    //        if (!File.Exists(filePath))
    //            throw new IocpException(ProtocolCode.FileNotExist, filePath);
    //        FileValdate.BeginInvoke(filePath, (result) =>
    //        {
    //            var md5Value = FileValdate.EndInvoke(result);
    //            var fileArgs = new FileTransferArgs(dirName, fileName)
    //            {
    //                Md5Value = md5Value
    //            };
    //            HandleUploadStart();
    //            var sendArgs = new CommandSendArgs(OperateTypes.UploadRequest)
    //                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
    //            SendCommandInWaiting(CommandTypes.TransferFile, sendArgs);
    //        }, null);
    //    }
    //    catch (Exception ex)
    //    {
    //        HandleException(ex);
    //    }
    //}

    public void Upload(string dirName, string fileName)
    {
        try
        {
            if (!AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.ProcessingFile);
            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileArgs = new FileTransferArgs(dirName, fileName)
            {
                Md5Value = fileStream.ToMd5HashString()
            };
            fileStream.Dispose();
            HandleUploadStart();
            var sender = new CommandSender(DateTime.Now, false, CommandTypes.TransferFile, OperateTypes.UploadRequest)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(nameof(Upload), ex);
        }
    }

    public void DownLoad(string dirName, string fileName)
    {
        try
        {
            if (!AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.ProcessingFile);
            var filePath = GetFileRepoPath(dirName, fileName);
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            var fileArgs = new FileTransferArgs(dirName, fileName)
            {
                Md5Value = fileStream.ToMd5HashString()
            };
            fileStream.Dispose();
            HandleDownloadStart();
            var sender = new CommandSender(DateTime.Now, false, CommandTypes.TransferFile, OperateTypes.DownloadRequest)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(nameof(DownLoad), ex);
        }
    }

    private void DoTransferFile(CommandReceiver receiver)
    {
        try
        {
            ReceiveCallback(receiver);
            switch (receiver.OperateType)
            {
                case OperateTypes.UploadRequest:
                    DoUploadRequest(receiver);
                    break;
                case OperateTypes.UploadContinue:
                    DoUpload(receiver);
                    break;
                case OperateTypes.DownloadRequest:
                    DoDownloadRequest(receiver);
                    break;
                case OperateTypes.DownloadContinue:
                    DoDownload(receiver);
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(nameof(DoTransferFile), ex);
        }
    }

    private void DoUploadRequest(CommandReceiver receiver)
    {
        var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        fileArgs.FileLength = AutoFile.Length;
        fileArgs.PacketLength = AutoFile.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : AutoFile.Length;
        var sender = new CommandSender(DateTime.Now, true, CommandTypes.TransferFile, OperateTypes.UploadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommand(sender);
    }

    private void DoUpload(CommandReceiver receiver)
    {
        var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        if (AutoFile.Position >= AutoFile.Length)
        {
            AutoFile.Dispose();
            HandleUploaded(fileArgs.StartTime);
            return;
        }
        var data = new byte[fileArgs.PacketLength];
        if (!AutoFile.Read(data, out var count))
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        HandleUploading(AutoFile.Length, AutoFile.Position);
        fileArgs.FileLength = AutoFile.Length;
        fileArgs.FilePosition = AutoFile.Position;
        var sender = new CommandSender(DateTime.Now, true, CommandTypes.TransferFile, OperateTypes.UploadContinue, data, 0, count)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommand(sender);
    }

    private void DoDownloadRequest(CommandReceiver receiver)
    {
        var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        File.Delete(filePath);
        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        var sender = new CommandSender(DateTime.Now, true, CommandTypes.TransferFile, OperateTypes.DownloadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommand(sender);
    }

    private void DoDownload(CommandReceiver receiver)
    {
        var fileArgs = receiver.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        AutoFile.Write(receiver.Data);
        // simple validation
        if (AutoFile.Position != fileArgs.FilePosition)
            throw new IocpException(ProtocolCode.NotSameVersion);
        fileArgs.FilePosition = AutoFile.Position;
        if (AutoFile.Position >= fileArgs.FileLength)
        {
            AutoFile.Dispose();
            HandleDownloaded(fileArgs.StartTime);
            var sender = new CommandSender(DateTime.Now, false, CommandTypes.TransferFile, OperateTypes.DownloadContinue)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommand(sender);
        }
        else
        {
            HandleDownloading(fileArgs.FileLength, AutoFile.Position);
            var sender = new CommandSender(DateTime.Now, true, CommandTypes.TransferFile, OperateTypes.DownloadContinue)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommand(sender);
        }
    }

    protected override string GetLog(string log)
    {
        return new StringBuilder()
                .Append(SignTable.OpenBracket)
                .Append(SocketInfo.LocalEndPoint)
                .Append(SignTable.CloseBracket)
                .Append(SignTable.Space)
                .Append(log)
                .ToString();
    }
}
