using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public class ServerProtocol : Protocol
{
    public ProtocolTypes Type { get; private set; } = ProtocolTypes.None;

    public string TimeStamp { get; } = DateTime.Now.ToString(DateTimeFormat.Data);

    public ServerProtocol()
    {
        DaemonThread = new(ConstTabel.SocketTimeoutMilliseconds, CheckTimeout);
        Commands[ProtocolKey.Login] = DoLogin;
        Commands[ProtocolKey.HeartBeats] = (_, _, _, _) => { };
        Commands[ProtocolKey.Upload] = DoUpload;
        Commands[ProtocolKey.WriteFile] = DoWriteFile;
        Commands[ProtocolKey.Download] = DoDownload;
        Commands[ProtocolKey.SendFile] = DoSendFile;
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

    private void CommandFail(Exception ex)
    {
        CommandFail(ex, new());
    }

    private void CommandFail(Exception ex, CommandComposer commandComposer)
    {
        if (ex is IocpException iocp)
            commandComposer.AppendFailure(iocp.ErrorCode, iocp.Message);
        else
            commandComposer.AppendFailure(ProtocolCode.UnknowError, ex.Message);
        WriteCommand(commandComposer);
        SendAsync();
        HandleException(ex);
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

    protected override void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            var commandKey = ProtocolKey.None;
            if (!commandParser.GetValueAsCommandKey(out commandKey))
                throw new IocpException(ProtocolCode.UnknownCommand);
            if (!CheckLogin(commandKey)) //检测登录
                throw new IocpException(ProtocolCode.NotLogined);
            if (!Commands.TryGetValue(commandKey, out var doCommand))
                throw new IocpException(ProtocolCode.UnknownCommand);
            doCommand(commandParser, buffer, offset, count);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    private bool CheckLogin(ProtocolKey commandKey)
    {
        if (commandKey is ProtocolKey.Login)
            return true;
        else
            return IsLogin;
    }

    private void DoLogin(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.UserName, out var name) ||
                !commandParser.GetValueAsString(ProtocolKey.Password, out var password) ||
                !commandParser.GetValueAsString(ProtocolKey.ProtocolType, out var t))
                throw new IocpException(ProtocolCode.ParameterError);
            var type = t.ToEnum<ProtocolTypes>();
            if (type is ProtocolTypes.None || Type is not ProtocolTypes.None && Type != type)
                throw new IocpException(ProtocolCode.WrongProtocolType);
            Type = type;
            if (type is not ProtocolTypes.HeartBeats)
                DaemonThread?.Stop();
            // TODO: validate userinfo
            UserInfo = new(name, password);
            IsLogin = true;
            HandleLogined();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Login);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    public void DoUpload(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.DirName, out var dirName) ||
                !commandParser.GetValueAsString(ProtocolKey.FileName, out var fileName) ||
                !commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !commandParser.GetValueAsLong(ProtocolKey.PacketLength, out var packetLength) ||
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
                throw new IocpException(ProtocolCode.ProcessingFile);
            HandleUploadStart();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.Position, 0);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }

    }

    private void DoWriteFile(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsLong(ProtocolKey.FileLength, out var fileLength) ||
                !commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength) ||
                !commandParser.GetValueAsLong(ProtocolKey.Position, out var position))
                throw new IocpException(ProtocolCode.ParameterError);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            AutoFile.Write(buffer, offset, count);
            // simple validation
            if (AutoFile.Position != position)
                throw new IocpException(ProtocolCode.NotSameVersion);
            if (AutoFile.Length >= fileLength)
            {
                // TODO: log success
                AutoFile.DisposeFileStream();
                HandleUploaded(startTime);
            }
            else
                HandleUploading(fileLength, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Upload)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    public void DoDownload(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.DirName, out var dirName) ||
                !commandParser.GetValueAsString(ProtocolKey.FileName, out var fileName) ||
                !commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime))
                throw new IocpException(ProtocolCode.ParameterError);
            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            var packetLength = fileStream.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : fileStream.Length;
            HandleDownloadStart();
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.FileLength, fileStream.Length)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.Position, 0);
            CommandSucceed(commandComposer);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
        }
    }

    public void Download(string dirName, string fileName, string startTime)
    {
        try
        {

            var filePath = GetFileRepoPath(dirName, fileName);
            if (!File.Exists(filePath))
                throw new IocpException(ProtocolCode.FileNotExist, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!AutoFile.Relocate(fileStream, ConstTabel.FileStreamExpireMilliseconds))
                throw new IocpException(ProtocolCode.ProcessingFile);
            var packetLength = fileStream.Length > ConstTabel.DataBytesTransferredMax ? ConstTabel.DataBytesTransferredMax : fileStream.Length;
            HandleDownloadStart();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void DoSendFile(CommandParser commandParser, byte[] buffer, int offset, int count)
    {
        try
        {
            if (!commandParser.GetValueAsString(ProtocolKey.StartTime, out var startTime) ||
                !commandParser.GetValueAsInt(ProtocolKey.PacketLength, out var packetLength))
                throw new IocpException(ProtocolCode.ParameterError);
            if (AutoFile.IsExpired)
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            if (AutoFile.Position >= AutoFile.Length)
            {
                // TODO: log success
                AutoFile.DisposeFileStream();
                HandleDownloaded(startTime);
                return;
            }
            buffer = new byte[packetLength];
            if (!AutoFile.Read(buffer, 0, buffer.Length, out count)) 
                throw new IocpException(ProtocolCode.FileExpired, startTime);
            HandleDownloading(AutoFile.Length, AutoFile.Position);
            var commandComposer = new CommandComposer()
                .AppendCommand(ProtocolKey.Download)
                .AppendValue(ProtocolKey.FileLength, AutoFile.Length)
                .AppendValue(ProtocolKey.StartTime, startTime)
                .AppendValue(ProtocolKey.PacketLength, packetLength)
                .AppendValue(ProtocolKey.Position, AutoFile.Position);
            CommandSucceed(commandComposer, buffer, 0, count);
        }
        catch (Exception ex)
        {
            CommandFail(ex);
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
