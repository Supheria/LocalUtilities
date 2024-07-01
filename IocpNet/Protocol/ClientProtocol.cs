using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public class ClientProtocol : IocpProtocol
{
    public IocpProtocolTypes Type { get; }

    AutoResetEvent ConnectDone { get; } = new(false);

    AutoResetEvent LoginDone { get; } = new(false);

    static int ResetSpan { get; } = 1000;

    bool IsConnect { get; set; } = false;

    protected override DaemonThread DaemonThread { get; }

    public ClientProtocol(IocpProtocolTypes type)
    {
        Type = type;
        DaemonThread = new(ConstTabel.HeartBeatsInterval, DoHeartBeats);
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
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Login)
                .AppendValue(ProtocolKey.UserName, UserInfo.Name)
                .AppendValue(ProtocolKey.Password, UserInfo.Password)
                .AppendValue(ProtocolKey.ProtocolType, Type);
            WriteCommand(commandComposer);
            SendAsync();
            LoginDone?.WaitOne(ResetSpan);
            if (Type is IocpProtocolTypes.HeartBeats)
                DaemonThread.Start();
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

    public void Operate(OperateTypes operate, string args)
    {
        try
        {
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Operate)
                .AppendValue(ProtocolKey.OperateType, operate);
            var count = WriteU8Buffer(args, out var buffer);
            WriteCommand(commandComposer, buffer, 0, count);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    protected override void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.Code, out var errorCode))
                throw new IocpException(ProtocolCode.UnknowError);
            var code = errorCode.ToEnum<ProtocolCode>();
            if (code is not ProtocolCode.Success)
            {
                if (commandParser.GetValueAsString(ProtocolKey.Message, out var message))
                    throw new IocpException(code, message);
                else
                    throw new IocpException(code);
            }
            commandParser.GetValueAsCommandKey(out var commandKey);
            switch (commandKey)
            {
                case ProtocolKey.Login:
                    DoLogin();
                    return;
                case ProtocolKey.HeartBeats:
                    DoHeartBeats();
                    return;
                case ProtocolKey.Operate:
                    DoOperate(commandParser, buffer, offset, count);
                    return;
                case ProtocolKey.Upload:
                    DoUpload(commandParser);
                    return;
                case ProtocolKey.Download:
                    DoDownload(commandParser, buffer, offset, count);
                    return;
                default:
                    throw new IocpException(ProtocolCode.UnknownCommand);
            };
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoLogin()
    {
        IsLogin = true;
        LoginDone.Set();
        HandleLogined();
    }

    private void DoHeartBeats()
    {
        try
        {
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.HeartBeats);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoOperate(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.OperateType, out var operate))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoOperate));
            var args = ReadU8Buffer(buffer, offset, count);
            HandleOperate(new(operate.ToEnum<OperateTypes>(), args));
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
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
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
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

    private void DoUpload(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoUpload));
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            if (AutoFile.Position >= AutoFile.Length)
            {
                // TODO: log success
                AutoFile.Close();
                HandleUploaded(startTime);
                return;
            }
            var buffer = new byte[packetLength];
            if (!AutoFile.Read(buffer, 0, buffer.Length, out var count))
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            HandleUploading(AutoFile.Length, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.WriteFile)
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

    public void Download(string dirName, string fileName, bool canRename)
    {
        try
        {
            if (!IsLogin)
                throw new IocpException(ProtocolCode.NotLogined);
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
            HandleDownloadStart();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.DirName, dirName)
                .AppendValue(ProtocolKey.FileName, fileName)
                .AppendValue(ProtocolKey.StartTime, DateTime.Now.ToString(DateTimeFormat.Data));
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoDownload(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsLong(ProtocolKey.FileLength, out var fileLength) ||
                !commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength) ||
                !commandParser.GetValueAsLong(ProtocolKey.Position, out var position))
                throw new IocpException(ProtocolCode.ParameterError, nameof(DoDownload));
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            AutoFile.Write(buffer, offset, count);
            // simple validation
            if (AutoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (AutoFile.Length >= fileLength)
            {
                // TODO: log success
                AutoFile.Close();
                HandleDownloaded(startTime);
            }
            else
                HandleDownloading(fileLength, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.SendFile)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            // TODO: log fail
        }
    }

    public override string GetLog(string message)
    {
        return new StringBuilder()
            .Append(SignTable.OpenParenthesis)
            .Append("client")
            .Append(SignTable.CloseParenthesis)
            .Append(SocketInfo.LocalEndPoint)
            .Append(SignTable.OpenParenthesis)
            .Append(UserInfo?.Name)
            .Append(SignTable.CloseParenthesis)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(message)
            .Append(SignTable.At)
            .Append(DateTime.Now.ToString(DateTimeFormat.Outlook))
            .ToString();
    }
}
