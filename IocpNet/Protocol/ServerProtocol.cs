using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeToolKit.Text;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

/// <summary>
/// 全功能处理协议
/// </summary>
/// <param name="server"></param>
/// <param name="userToken"></param>
public class ServerProtocol : IocpProtocol
{
    public event IocpEventHandler? OnFileReceived;

    public event IocpEventHandler? OnFileSent;

    public event IocpEventHandler<Exception>? OnException;

    public event IocpEventHandler<string>? OnMessage;

    object AcceptLocker { get; } = new();

    public bool ProcessAccept(Socket? acceptSocket)
    {
        lock (AcceptLocker)
        {
            if (acceptSocket is null || Socket is not null)
                return false;
            Socket = acceptSocket;
            SocketInfo.Connect(acceptSocket);
            return true;
        }
    }

    public override void SendMessage(string message)
    {
        var commandComposer = new CommandComposer()
            .AppendCommand(ProtocolKey.Message);
        var buffer = Encoding.UTF8.GetBytes(message);
        CommandSucceed(commandComposer, buffer, 0, buffer.Length);
    }

    /// <summary>
    /// 处理分完包的数据，子类从这个方法继承,服务端在此处处理所有的客户端命令请求
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    protected override void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        if (!commandParser.GetValueAsCommandKey(out var commandKey))
            return;
        if (!CheckLogin(commandKey)) //检测登录
        {
            CommandFail(ProtocolCode.UserHasLogined, "");
            return;
        }
        switch (commandKey)
        {
            case ProtocolKey.Login:
                DoLogin(commandParser);
                return;
            case ProtocolKey.Message:
                DoMessage(buffer, offset, count);
                return;
            case ProtocolKey.Upload:
                DoUpload(commandParser);
                return;
            case ProtocolKey.WriteFile:
                DoWriteFile(commandParser, buffer, offset, count);
                return;
            case ProtocolKey.Download:
                DoDownload(commandParser);
                return;
            case ProtocolKey.SendFile:
                DoSendFile(commandParser);
                return;
            default:
                return;
        }
    }

    private bool CheckLogin(ProtocolKey commandKey)
    {
        if (commandKey is ProtocolKey.Login)
            return true;
        else
            return IsLogin;
    }

    private void CommandFail(ProtocolCode errorCode, string message)
    {
        CommandFail(errorCode, message, new());
    }

    private void CommandFail(ProtocolCode errorCode, string message, CommandComposer commandComposer)
    {
        commandComposer.AppendFailure(errorCode, message);
        SendCommand(commandComposer);
    }

    private void CommandSucceed(CommandComposer commandComposer)
    {
        CommandSucceed(commandComposer, [], 0, 0);
    }

    private void CommandSucceed(CommandComposer commandComposer, byte[] buffer, int offset, int count)
    {
        commandComposer.AppendSuccess();
        SendCommand(commandComposer, buffer, offset, count);
    }

    /// <summary>
    /// 处理客户端文件上传
    /// </summary>
    /// <returns></returns>
    public void DoUpload(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.TargetPath, out var targetPath) ||
                !commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp) ||
                !commandParser.GetValueAsLong(ProtocolKey.PacketSize, out var packetSize) ||
                !commandParser.GetValueAsBool(ProtocolKey.CanRename, out var canRename))
                throw new IocpException(ProtocolCode.ParameterError, "");
            var dir = Path.GetDirectoryName(targetPath) ?? throw new IocpException(ProtocolCode.ParameterInvalid, targetPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(targetPath))
            {
                if (!canRename)
                    throw new IocpException(ProtocolCode.FileAlreadyExist, targetPath);
                targetPath = targetPath.RenamePathByDateTime();
            }
            var fileStream = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            var autoFile = new AutoDisposeFileStream(stamp, fileStream, ConstTabel.FileStreamExpireMilliseconds);
            autoFile.OnClosed += (file) => FileWriters.Remove(file.TimeStamp);
            FileWriters[autoFile.TimeStamp] = autoFile;
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.Position, 0);
            CommandSucceed(commandComposer);
            return;
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
    }

    private void DoWriteFile(CommandParser commandParser, byte[] buffer, int offset, int count)
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
            // simple validation
            if (autoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (autoFile.Length >= fileLength)
            {
                // TODO: log success
                autoFile.Close();
                OnFileReceived?.InvokeAsync(this);
            }
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize);
            CommandSucceed(commandComposer);
            return;
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
    }

    /// <summary>
    /// 处理客户端文件下载
    /// </summary>
    /// <returns></returns>
    public void DoDownload(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.TargetPath, out var targetPath) ||
                !commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp))
                throw new IocpException(ProtocolCode.ParameterError);
            if (!File.Exists(targetPath))
                throw new IocpException(ProtocolCode.FileNotExist, targetPath);
            var fileStream = new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var autoFile = new AutoDisposeFileStream(stamp, fileStream, ConstTabel.FileStreamExpireMilliseconds);
            autoFile.OnClosed += (file) => FileReaders.Remove(file.TimeStamp);
            FileReaders[stamp] = autoFile;
            var packetSize = fileStream.Length > ConstTabel.TransferBufferMax ? ConstTabel.TransferBufferMax : fileStream.Length;
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.FileLength, fileStream.Length)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.Position, 0);
            CommandSucceed(commandComposer);

        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
    }

    private void DoSendFile(CommandParser commandParser)
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
                OnFileSent?.InvokeAsync(this);
                return;
            }
            var buffer = new byte[packetSize];
            if (!autoFile.Read(buffer, 0, buffer.Length, out var count))
                throw new IocpException(ProtocolCode.FileIsExpired);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.FileLength, autoFile.Length)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.Position, autoFile.Position);
            SendCommand(commandComposer, buffer, 0, count);
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
    }

    private void DoMessage(byte[] buffer, int offset, int count)
    {
        var message = Encoding.UTF8.GetString(buffer, offset, count);
        OnMessage?.InvokeAsync(this, message);
        var commandComposer = new CommandComposer()
            .AppendCommand(ProtocolKey.Message);
        CommandSucceed(commandComposer);
    }

    private void DoLogin(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.UserName, out var name) ||
                !commandParser.GetValueAsString(ProtocolKey.Password, out var password))
                throw new IocpException(ProtocolCode.ParameterError);
            var success = name == "admin" && password == "password".ToMd5HashString();
            if (!success)
                throw new IocpException(ProtocolCode.UserOrPasswordError);
            UserInfo = new(name, password);
            IsLogin = true;
            // TODO: log success
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Login)
                .AppendValue(ProtocolKey.UserId, UserInfo.Id)
                .AppendValue(ProtocolKey.UserName, UserInfo.Name);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            OnException?.InvokeAsync(this, ex);
            // TODO: log fail
        }
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
