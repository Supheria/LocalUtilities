using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public class HostProtocol : IocpProtocol
{
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
        WriteCommand(commandComposer);
        SendAsync();
    }

    private void CommandSucceed(CommandComposer commandComposer)
    {
        CommandSucceed(commandComposer, [], 0, 0);
    }

    private void CommandSucceed(CommandComposer commandComposer, byte[] buffer, int offset, int count)
    {
        commandComposer.AppendSuccess();
        WriteCommand(commandComposer, buffer, offset, count);
        SendAsync();
    }

    public void DoUpload(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.DirName, out var dirName) ||
                !commandParser.GetValueAsString(ProtocolKey.FileName, out var fileName) ||
                !commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp) ||
                !commandParser.GetValueAsLong(ProtocolKey.PacketSize, out var packetSize) ||
                !commandParser.GetValueAsBool(ProtocolKey.CanRename, out var canRename))
                throw new IocpException(ProtocolCode.ParameterError, "");
            var filePath = GetFileRepoPath(dirName, fileName);
            if (File.Exists(filePath))
            {
                if (!canRename)
                    throw new IocpException(ProtocolCode.FileAlreadyExist, filePath);
                filePath = filePath.RenamePathByDateTime();
            }
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.FileInProcess);
            HandleUploadStart();
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
            HandleException(ex);
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
                HandleUploaded(DateTime.Now - start);
            }
            else
                HandleUploading(fileLength, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            HandleException(ex);
            // TODO: log fail
        }
    }

    public void DoDownload(CommandParser commandParser)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.DirName, out var dirName) ||
                !commandParser.GetValueAsString(ProtocolKey.FileName, out var fileName) ||
                !commandParser.GetValueAsString(ProtocolKey.Stamp, out var stamp))
                throw new IocpException(ProtocolCode.ParameterError);
            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.FileInProcess);
            var packetSize = fileStream.Length > ConstTabel.TransferBufferMax ? ConstTabel.TransferBufferMax : fileStream.Length;
            HandleDownloadStart();
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
            HandleException(ex);
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
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, stamp);
            if (AutoFile.Position >= AutoFile.Length)
            {
                // TODO: log success
                AutoFile.Close();
                _ = DateTime.TryParse(stamp, out var start);
                HandleDownloaded(DateTime.Now - start);
                return;
            }
            var buffer = new byte[packetSize];
            if (!AutoFile.Read(buffer, 0, buffer.Length, out var count))
                throw new IocpException(ProtocolCode.FileExpired, stamp);
            HandleDownloading(AutoFile.Length, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.FileLength, AutoFile.Length)
                .AppendValue(ProtocolKey.Stamp, stamp)
                .AppendValue(ProtocolKey.PacketSize, packetSize)
                .AppendValue(ProtocolKey.Position, AutoFile.Position);
            CommandSucceed(commandComposer, buffer, 0, count);
        }
        catch (Exception ex)
        {
            if (ex is IocpException iocp)
                CommandFail(iocp.ErrorCode, iocp.Message);
            else
                CommandFail(ProtocolCode.UnknowError, ex.Message);
            HandleException(ex);
            // TODO: log fail
        }
    }

    private void DoMessage(byte[] buffer, int offset, int count)
    {
        var message = Encoding.UTF8.GetString(buffer, offset, count);
        HandleMessage(message);
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
            HandleLogined();
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
            HandleException(ex);
            // TODO: log fail
        }
    }

    public override string GetLog(string message)
    {
        return new StringBuilder()
            .Append(SocketInfo.RemoteEndPoint)
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
