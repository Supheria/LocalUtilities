using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeToolKit.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public class ClientProtocol : IocpProtocol
{
    public event IocpEventHandler? OnConnect;

    public event IocpEventHandler? OnUploaded;

    public event IocpEventHandler? OnDownloaded;

    public event IocpEventHandler<float>? OnUploading;

    public event IocpEventHandler<float>? OnDownloading;

    public event IocpEventHandler<Exception>? OnException;

    public event IocpEventHandler<string>? OnMessage;

    AutoResetEvent ConnectDone { get; } = new(false);

    object ConnectLocker { get; } = new();

    bool IsConnect { get; set; } = false;

    public void Login(string host, int port, string name, string password)
    {
        try
        {
            Connect(new IPEndPoint(IPAddress.Parse(host), port));
            if (!IsConnect)
                throw new IocpException(ProtocolCode.NoConnection);
            UserInfo = new(name, password);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Login)
                .AppendValue(ProtocolKey.UserName, UserInfo.Name)
                .AppendValue(ProtocolKey.Password, UserInfo.Password);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            Close();
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
            //Logger.Error("AsyncClientFullHandlerSocket.DoLogin" + "userID:" + userID + " password:" + password + " " + E.Message);
        }
    }

    private void Connect(IPEndPoint remoteEndPoint)
    {
        lock (ConnectLocker)
        {
            try
            {
                if (Socket is not null && Socket.RemoteEndPoint?.ToString() == remoteEndPoint.ToString())
                    return;
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
                ConnectDone.WaitOne(1000);
            }
            catch (Exception ex)
            {
                Close();
                //Console.WriteLine(ex.ToString());
            }
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
        OnConnect?.InvokeAsync(this);
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
            OnException?.InvokeAsync(this, ex);
        }
    }

    protected override void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        commandParser.GetValueAsInt(ProtocolKey.Code, out var errorCode);
        if ((ProtocolCode)errorCode is not ProtocolCode.Success)
            // TODO: log fail
            // TODO: handle error code
            return;
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

    private void DoConnect()
    {
        ConnectDone.Set();
    }

    private void DoMessage(byte[] buffer, int offset, int count)
    {
        string message = Encoding.UTF8.GetString(buffer, offset, count);
        if (!string.IsNullOrWhiteSpace(message))
        {
            OnMessage?.InvokeAsync(this, message);
        }
    }

    private void DoLogin()
    {
        IsLogin = true;
        OnMessage?.InvokeAsync(this, $"{UserInfo?.Name} logined");
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
            if (!FileWriters.TryGetValue(stamp, out var autoFile))
                throw new IocpException(ProtocolCode.ParameterInvalid, "invalid file stamp");
            autoFile.Write(buffer, offset, count);
            OnDownloading?.InvokeAsync(this, autoFile.Position * 100f / fileLength);
            // simple validation
            if (autoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (autoFile.Length >= fileLength)
            {
                autoFile.Close();
                OnDownloaded?.InvokeAsync(this);
            }
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.SendFile)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize);
            WriteCommand(commandComposer);
            SendAsync();
        }
        catch (Exception ex)
        {
            OnException?.InvokeAsync(this, ex);
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
            if (!FileReaders.TryGetValue(stamp, out var autoFile))
                throw new IocpException(ProtocolCode.ParameterInvalid, "invalid file stamp");
            if (autoFile.Position >= autoFile.Length)
            {
                // TODO: log success
                autoFile.Close();
                OnUploaded?.InvokeAsync(this);
                return;
            }
            OnUploading?.Invoke(this, autoFile.Position * 100f / autoFile.Length);
            var buffer = new byte[packetSize];
            if (!autoFile.Read(buffer, 0, buffer.Length, out var count))
                throw new IocpException(ProtocolCode.FileExpired);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.WriteFile)
                .AppendValue(ProtocolKey.FileLength, autoFile.Length)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.Position, autoFile.Position);
            WriteCommand(commandComposer, buffer, 0, count);
            SendAsync();
        }
        catch (Exception ex)
        {
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
    }

    public void Upload(string dirName, string fileName, bool canRename)
    {
        try
        {
            if (!IsLogin)
                throw new IocpException(ProtocolCode.NotLogined);
            var filePath = Path.Combine(RootDirectory, dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var stamp = DateTime.Now.ToString();
            var autoFile = new AutoDisposeFileStream(stamp, fileStream, ConstTabel.FileStreamExpireMilliseconds);
            autoFile.OnClosed += (file) => FileReaders.Remove(file.TimeStamp);
            FileReaders[stamp] = autoFile;
            var packetSize = fileStream.Length > ConstTabel.TransferBufferMax ? ConstTabel.TransferBufferMax : fileStream.Length;
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
            OnException?.InvokeAsync(this, ex);
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
            var dir = Path.Combine(RootDirectory, dirName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, fileName);
            if (File.Exists(filePath))
            {
                if (!canRename)
                    throw new IocpException(ProtocolCode.FileAlreadyExist, filePath);
                filePath = filePath.RenamePathByDateTime();
            }
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            var stamp = DateTime.Now.ToString();
            var autoFile = new AutoDisposeFileStream(stamp, fileStream, ConstTabel.FileStreamExpireMilliseconds);
            autoFile.OnClosed += (file) => FileWriters.Remove(file.TimeStamp);
            FileWriters[stamp] = autoFile;
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
            OnException?.InvokeAsync(this, ex);
            //记录日志
            //Logger.Error(E.Message);
        }
    }
}
