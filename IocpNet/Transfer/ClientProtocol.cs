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
        Commands[CommandTypes.Upload] = DoUpload;
        Commands[CommandTypes.Download] = DoDownload;
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
            if (!IsLogin)
                throw new IocpException(ProtocolCode.NotLogined);
            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            var packetLength = fileStream.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : fileStream.Length;
            HandleUploadStart();
            var commandComposer = new Command(CommandTypes.Upload)
                .AppendValue(ProtocolKey.DirName, dirName)
                .AppendValue(ProtocolKey.FileName, fileName)
                .AppendValue(ProtocolKey.StartTime, DateTime.Now.ToString(DateTimeFormat.Data))
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.CanRename, canRename);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoUpload(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!command.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !command.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoUpload));
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            if (AutoFile.Position >= AutoFile.Length)
            {
                // TODO: log success
                AutoFile.DisposeFileStream();
                HandleUploaded(startTime);
                return;
            }
            buffer = new byte[packetLength];
            if (!AutoFile.Read(buffer, 0, buffer.Length, out count))
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            HandleUploading(AutoFile.Length, AutoFile.Position);
            var commandComposer = new Command(CommandTypes.WriteFile)
                .AppendValue(ProtocolKey.FileLength, AutoFile.Length)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.Position, AutoFile.Position);
            WriteCommand(commandComposer, buffer, 0, count);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    public void DownLoad(string dirName, string fileName, bool canRename)
    {
        try
        {
            if (!AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.ProcessingFile);
            var downloadArgs = new DownloadArgs(DateTime.Now, dirName, fileName, canRename);
            var sendArgs = new OperateSendArgs(OperateTypes.DownloadRequest, downloadArgs.ToSs());
            SendCommandInWaiting(CommandTypes.Download, sendArgs);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoDownload(Command command, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!ReceiveCallback(command, out var callbackArgs))
                return;
            var downloadArgs = new DownloadArgs().ParseSs(callbackArgs.Data);
            if (AutoFile.IsExpired)
            {
                RelocateDownloadFile(downloadArgs);
                HandleDownloadStart();
            }
            AutoFile.Write(buffer, offset, count);
            // simple validation
            if (AutoFile.Position != downloadArgs.FilePosition)
                throw new IocpException(ProtocolCode.NotSameVersion);
            downloadArgs.FilePosition = AutoFile.Position;
            var sendArgd = new OperateSendArgs(OperateTypes.DownloadContinue, downloadArgs.ToSs());
            if (AutoFile.Length >= downloadArgs.FileLength)
            {
                AutoFile.DisposeFileStream();
                HandleDownloaded(downloadArgs.StartTime);
                SendCommand(CommandTypes.Download, sendArgd);
            }
            else
            {
                HandleDownloading(downloadArgs.FileLength, AutoFile.Position);
                SendCommandInWaiting(CommandTypes.Download, sendArgd);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void RelocateDownloadFile(DownloadArgs args)
    {
        var filePath = GetFileRepoPath(args.DirName, args.FileName);
        if (File.Exists(filePath))
        {
            if (!args.CanRename)
                throw new IocpException(ProtocolCode.FileAlreadyExist, filePath);
            filePath = filePath.RenamePathByDateTime();
        }
        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
            throw new IocpException(ProtocolCode.ProcessingFile);
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
