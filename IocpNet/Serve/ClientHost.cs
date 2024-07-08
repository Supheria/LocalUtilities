using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeGeneral;
using System.Net;

namespace LocalUtilities.IocpNet.Serve;

public class ClientHost : Host
{
    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public event IocpEventHandler<string>? OnProcessing;

    public event IocpEventHandler<string[]>? OnUpdateUserList;

    ClientProtocol HeartBeats { get; } = new(ProtocolTypes.HeartBeats);

    ClientProtocol Operator { get; } = new(ProtocolTypes.Operator);

    ClientProtocol Upload { get; } = new(ProtocolTypes.Upload);

    ClientProtocol Download { get; } = new(ProtocolTypes.Download);

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    public ClientHost()
    {
        HeartBeats.OnLog += HandleLog;
        HeartBeats.OnLogined += () => OnConnected?.Invoke();
        HeartBeats.OnClosed += () => OnDisconnected?.Invoke();
        Operator.OnLog += HandleLog;
        Operator.OnOperate += DoOperate;
        Operator.OnOperateCallback += DoOperateCallback;
        Upload.OnLog += HandleLog;
        Upload.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
        Download.OnLog += HandleLog;
        Download.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
    }

    public void Connect(string address, int port, string name, string password)
    {
        Host = new(IPAddress.Parse(address), port);
        UserInfo = new(name, password);
        HeartBeats.Connect(Host, UserInfo);
        Operator.Connect(Host, UserInfo);
        Upload.Connect(Host, UserInfo);
        Download.Connect(Host, UserInfo);
    }

    public void Close()
    {
        Host = null;
        UserInfo = null;
        HeartBeats.Dispose();
        Operator.Dispose();
        Upload.Dispose();
        Download.Dispose();
    }

    private void DoOperate(CommandReceiver receiver)
    {
        var sender = new CommandSender(receiver.TimeStamp, CommandTypes.OperateCallback, receiver.OperateType);
        try
        {
            switch (receiver.OperateType)
            {
                case OperateTypes.Message:
                    HandleMessage(receiver);
                    break;
                case OperateTypes.UpdateUserList:
                    var userList = ReadU8Buffer(receiver.Data).ToArray();
                    OnUpdateUserList?.Invoke(userList);
                    break;
            }
            Operator.CallbackSuccess(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            Operator.CallbackFailure(sender, ex);
        }
    }

    private void DoOperateCallback(CommandReceiver receiver)
    {
        try
        {
            switch (receiver.OperateType)
            {
                case OperateTypes.Message:
                    break;
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void SendMessage(string message, string userName)
    {
        try
        {
            if (UserInfo is null)
                throw new IocpException(ProtocolCode.EmptyUserInfo);
            var count = WriteU8Buffer(message, out var data);
            var sender = new CommandSender(DateTime.Now, CommandTypes.Operate, OperateTypes.Message, data, 0, count)
                .AppendArgs(ProtocolKey.ReceiveUser, userName)
                .AppendArgs(ProtocolKey.SendUser, UserInfo.Name);
            Operator.SendCommand(sender);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void UploadFile(string dirName, string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var localPath = Upload.GetFileRepoPath(dirName, fileName);
            if (!File.Exists(localPath))
                File.Copy(filePath, localPath);
            Upload.Upload(dirName, fileName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void DownloadFile(string dirName, string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            Download.DownLoad(dirName, fileName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
}
