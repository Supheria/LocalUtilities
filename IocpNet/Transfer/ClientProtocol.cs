using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.IO;
using System.IO.Pipes;
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
        //Commands[CommandTypes.Upload] = DoUpload;
        //Commands[CommandTypes.Download] = DoDownload;
    }

    private void HeartBeats()
    {
        try
        {
            var commandComposer = new Command(CommandTypes.HeartBeats, null);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
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
            var sendArgs = new OperateSendArgs(OperateTypes.None)
                .AppendArgs(ProtocolKey.UserName, userInfo.Name ?? "")
                .AppendArgs(ProtocolKey.Password, userInfo.Password)
                .AppendArgs(ProtocolKey.ProtocolType, Type.ToString());
            SendCommandInWaiting(CommandTypes.Login, sendArgs);
            LoginDone?.WaitOne(ResetSpan);
            DaemonThread?.Start();
        }
        catch (Exception ex)
        {
            Close();
            HandleException(ex);
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

    protected override void ProcessCommand(Command command)
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
            if (!Commands.TryGetValue(command.Type, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(command);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoLogin(Command command)
    {
        if (!ReceiveCallback(command, out _))
            return;
        IsLogin = true;
        LoginDone.Set();
        HandleLogined();
    }

    public void Upload(string dirName, string fileName, bool canRename)
    {
        try
        {
            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileArgs = new FileTransferArgs(dirName, fileName, fileStream.ToMd5HashString());
            fileStream.Dispose();
            HandleUploadStart();
            var sendArgs = new OperateSendArgs(OperateTypes.UploadRequest)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommandInWaiting(CommandTypes.TransferFile, sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
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
            var fileArgs = new FileTransferArgs(dirName, fileName, fileStream.ToMd5HashString());
            fileStream.Dispose();
            HandleDownloadStart();
            var sendArgs = new OperateSendArgs(OperateTypes.DownloadRequest)
                .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
            SendCommandInWaiting(CommandTypes.TransferFile, sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoTransferFile(Command command)
    {
        try
        {
            if (!ReceiveCallback(command, out var callbackArgs))
                return;
            var fileArgs = callbackArgs.GetArgs<FileTransferArgs>(ProtocolKey.FileTransferArgs);
            switch (callbackArgs.Type)
            {
                case OperateTypes.UploadRequest:
                    DoUploadRequest(fileArgs);
                    break;
                case OperateTypes.UploadContinue:
                    DoUpload(fileArgs);
                    break;
                case OperateTypes.DownloadRequest:
                    DoDownloadRequest(fileArgs);
                    break;
                case OperateTypes.DownloadContinue:
                    DoDownload(fileArgs, command.Data);
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoUploadRequest(FileTransferArgs fileArgs)
    {
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        fileArgs.FileLength = AutoFile.Length;
        fileArgs.PacketLength = AutoFile.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : AutoFile.Length;
        var sendArgs = new OperateSendArgs(OperateTypes.UploadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommandInWaiting(CommandTypes.TransferFile, sendArgs);
    }

    private void DoUpload(FileTransferArgs fileArgs)
    {
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
        var sendArgs = new OperateSendArgs(OperateTypes.UploadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommandInWaiting(CommandTypes.TransferFile, sendArgs, data, 0, count);
    }

    private void DoDownloadRequest(FileTransferArgs fileArgs)
    {
        var filePath = GetFileRepoPath(fileArgs.DirName, fileArgs.FileName);
        File.Delete(filePath);
        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        if (!AutoFile.Relocate(fileStream))
            throw new IocpException(ProtocolCode.ProcessingFile);
        var sendArgd = new OperateSendArgs(OperateTypes.DownloadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        SendCommandInWaiting(CommandTypes.TransferFile, sendArgd);
    }

    private void DoDownload(FileTransferArgs fileArgs, byte[] buffer)
    {
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, fileArgs.FileName);
        AutoFile.Write(buffer);
        // simple validation
        if (AutoFile.Position != fileArgs.FilePosition)
            throw new IocpException(ProtocolCode.NotSameVersion);
        fileArgs.FilePosition = AutoFile.Position;
        var sendArgd = new OperateSendArgs(OperateTypes.DownloadContinue)
            .AppendArgs(ProtocolKey.FileTransferArgs, fileArgs.ToSsString());
        if (AutoFile.Position >= fileArgs.FileLength)
        {
            AutoFile.Dispose();
            HandleDownloaded(fileArgs.StartTime);
            SendCommand(CommandTypes.TransferFile, sendArgd);
        }
        else
        {
            HandleDownloading(fileArgs.FileLength, AutoFile.Position);
            SendCommandInWaiting(CommandTypes.TransferFile, sendArgd);
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
