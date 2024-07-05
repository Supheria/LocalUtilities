using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class ClientProtocol : Protocol
{
    public ProtocolTypes Type { get; }

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
            var commandComposer = new Command(CommandTypes.HeartBeats);
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
            var command = new Command(CommandTypes.Login)
                .AppendValue(ProtocolKey.UserName, UserInfo.Name)
                .AppendValue(ProtocolKey.Password, UserInfo.Password)
                .AppendValue(ProtocolKey.ProtocolType, Type);
            WriteCommand(command);
            SendAsync();
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

    protected override void ProcessCommand(Command command, byte[] buffer, int offset, int count)
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
            doCommand(command, buffer, offset, count);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoLogin(Command command, byte[] buffer, int offset, int count)
    {
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
            if (!AutoFile.Relocate(fileStream))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleUploadStart();
            var fileArgs = new FileProcessArgs(dirName, fileName)
            {
                PacketLength = AutoFile.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : AutoFile.Length
            };
            var sendArgs = new OperateSendArgs(OperateTypes.UploadRequest, fileArgs.ToSs());
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
            if (File.Exists(filePath))
            {
                // TODO: send md5 valiedate
                filePath = filePath.RenamePathByDateTime();
            }
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (!AutoFile.Relocate(fileStream))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleDownloadStart();
            var fileArgs = new FileProcessArgs(dirName, fileName);
            var sendArgs = new OperateSendArgs(OperateTypes.DownloadRequest, fileArgs.ToSs());
            SendCommandInWaiting(CommandTypes.TransferFile, sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoTransferFile(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!ReceiveCallback(command, out var callbackArgs))
                return;
            var fileArgs = new FileProcessArgs().ParseSs(callbackArgs.Args);
            switch (callbackArgs.Type)
            {
                case OperateTypes.UploadRequest:
                case OperateTypes.UploadContinue:
                    DoUpload(fileArgs);
                    break;
                case OperateTypes.DownloadRequest:
                case OperateTypes.DownloadContinue:
                    DoDownload(fileArgs, buffer, offset, count);
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoUpload(FileProcessArgs args)
    {
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, args.FileName);
        if (AutoFile.Position >= AutoFile.Length)
        {
            AutoFile.DisposeFileStream();
            HandleUploaded(args.StartTime);
            return;
        }
        var buffer = new byte[args.PacketLength];
        if (!AutoFile.Read(buffer, 0, buffer.Length, out var count))
            throw new IocpException(ProtocolCode.FileExpired, args.FileName);
        HandleUploading(AutoFile.Length, AutoFile.Position);
        args.FileLength = AutoFile.Length;
        args.FilePosition = AutoFile.Position;
        var sendArgs = new OperateSendArgs(OperateTypes.UploadContinue, args.ToSs());
        SendCommandInWaiting(CommandTypes.TransferFile, sendArgs, buffer, 0, count);
    }

    private void DoDownload(FileProcessArgs args, byte[] buffer, int offset, int count)
    {
        if (AutoFile.IsExpired)
            throw new IocpException(ProtocolCode.FileExpired, args.FileName);
        AutoFile.Write(buffer, offset, count);
        // simple validation
        if (AutoFile.Position != args.FilePosition)
            throw new IocpException(ProtocolCode.NotSameVersion);
        args.FilePosition = AutoFile.Position;
        var sendArgd = new OperateSendArgs(OperateTypes.DownloadContinue, args.ToSs());
        if (AutoFile.Length >= args.FileLength)
        {
            AutoFile.DisposeFileStream();
            HandleDownloaded(args.StartTime);
            SendCommand(CommandTypes.TransferFile, sendArgd);
        }
        else
        {
            HandleDownloading(args.FileLength, AutoFile.Position);
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
