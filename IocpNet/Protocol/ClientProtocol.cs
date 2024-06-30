using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Mathematic;
using LocalUtilities.TypeToolKit.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public class ClientProtocol : IocpProtocol
{
    AutoResetEvent ConnectDone { get; } = new(false);

    AutoResetEvent LoginDone { get; } = new(false);

    static int ResetSpan { get; } = 1000;

    bool IsConnect { get; set; } = false;

    public void Connect(IPEndPoint? host, UserInfo? userInfo)
    {
        try
        {
            if (IsLogin && SocketInfo.RemoteEndPoint?.ToString() == host?.ToString())
                return;
            Connect(host);
            if (!IsConnect)
                throw new IocpException(ProtocolCode.NoConnection);
            UserInfo = userInfo;
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Login)
                .AppendValue(ProtocolKey.UserName, UserInfo.Name)
                .AppendValue(ProtocolKey.Password, UserInfo.Password);
            WriteCommand(commandComposer);
            SendAsync();
            LoginDone?.WaitOne(ResetSpan);
        }
        catch (Exception ex)
        {
            Close();
            HandleException(ex);
            // TODO: log fail
            //Logger.Error("AsyncClientFullHandlerSocket.DoLogin" + "userID:" + userID + " password:" + password + " " + E.Message);
        }
    }

    private void Connect(IPEndPoint? remoteEndPoint)
    {
        try
        {
            Close();
            IsConnect = false;
            var connectArgs = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = remoteEndPoint,
            };
            connectArgs.Completed += (_, args) => ProcessConnect(args);
            Socket ??= new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!Socket.ConnectAsync(connectArgs))
                ProcessConnect(connectArgs);
            ConnectDone.WaitOne(ResetSpan);
        }
        catch (Exception ex)
        {
            Close();
            //Console.WriteLine(ex.ToString());
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

    public override void SendMessage(string message)
    {
        try
        {
            if (!IsLogin)
                throw new IocpException(ProtocolCode.NotLogined);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Message);
            var buffer = Encoding.UTF8.GetBytes(message);
            WriteCommand(commandComposer, buffer, 0, buffer.Length);
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
                case ProtocolKey.Active:
                    DoConnect();
                    return;
                case ProtocolKey.Login:
                    DoLogin();
                    return;
                case ProtocolKey.Message:
                    DoMessage(buffer, offset, count);
                    return;
                case ProtocolKey.Upload:
                    DoUpload(commandParser);
                    return;
                case ProtocolKey.Download:
                    DoDownload(commandParser, buffer, offset, count);
                    return;
                default:
                    return;
            };
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoConnect()
    {
        ConnectDone.Set();
    }

    private void DoMessage(byte[] buffer, int offset, int count)
    {
        string message = Encoding.UTF8.GetString(buffer, offset, count);
        HandleMessage(message);
    }

    private void DoLogin()
    {
        IsLogin = true;
        LoginDone.Set();
        HandleLogined();
    }

    private void DoDownload(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsLong(ProtocolKey.FileLength, out var fileLength) ||
                !commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketSize, out var packetSize) ||
                !commandParser.GetValueAsLong(ProtocolKey.Position, out var position))
                throw new IocpException(ProtocolCode.ParameterError);
            if (AutoFile.IsExpired) 
                throw new IocpException(ProtocolCode.FileExpired, stamp);
            AutoFile.Write(buffer, offset, count);
            // simple validation
            if (AutoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (AutoFile.Length >= fileLength)
            {
                // TODO: log success
                AutoFile.Close();
                _ = DateTime.TryParse(stamp, out var start);
                HandleDownloaded(DateTime.Now - start);
            }
            else
                HandleDownloading(fileLength, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.SendFile)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize);
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
            if (!commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketSize, out var packetSize))
                throw new IocpException(ProtocolCode.ParameterError);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, stamp);
            if (AutoFile.Position >= AutoFile.Length)
            {
                // TODO: log success
                AutoFile.Close();
                _ = DateTime.TryParse(stamp, out var start);
                HandleUploaded(DateTime.Now - start);
                return;
            }
            var buffer = new byte[packetSize];
            if (!AutoFile.Read(buffer, 0, buffer.Length, out var count))
                throw new IocpException(ProtocolCode.FileExpired, stamp);
            HandleUploading(AutoFile.Length, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.WriteFile)
                .AppendValue(ProtocolKey.FileLength, AutoFile.Length)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
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
            var stamp = DateTime.Now.ToString();
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            var packetSize = fileStream.Length > ConstTabel.TransferBufferMax ? ConstTabel.TransferBufferMax : fileStream.Length;
            HandleUploadStart();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.DirName, dirName)
                .AppendValue(ProtocolKey.FileName, fileName)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.CanRename, canRename);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            //记录日志
            //Logger.Error(e.Message);
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
            var stamp = DateTime.Now.ToString();
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleDownloadStart();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.DirName, dirName)
                .AppendValue(ProtocolKey.FileName, fileName)
                .AppendValue(ProtocolKey.Stamp, stamp);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            //记录日志
            //Logger.Error(E.Message);
        }
    }

    public override string GetLog(string message)
    {
        return new StringBuilder()
            .Append(SocketInfo.LocalEndPoint)
            .Append(SignTable.Open)
            .Append(UserInfo?.Name)
            .Append(SignTable.Close)
            .Append(SignTable.Mark)
            .Append(SignTable.Space)
            .Append(message)
            .Append(SignTable.At)
            .Append(DateTime.Now.ToUiniformLook())
            .ToString();
    }
}
