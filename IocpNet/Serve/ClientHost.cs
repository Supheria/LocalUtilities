using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using System.Net;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class ClientHost : Host
{
    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public event IocpEventHandler<string>? OnProcessing;

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
        Operator.OnOperate += ReceiveOperate;
        Operator.OnOperateCallback += ReceiveOperateCallback;
        Upload.OnLog += HandleLog;
        Upload.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
        Download.OnLog += HandleLog;
        Download.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
    }

    private void ReceiveOperate(Command command)
    {
        var sendArgs = command.GetOperateSendArgs();
        switch (sendArgs.Type)
        {
            case OperateTypes.Message:
                ReceiveMessage(sendArgs, command.Data);
                break;
        }
    }

    private void ReceiveMessage(OperateSendArgs sendArgs, byte[] data)
    {
        var message = Encoding.UTF8.GetString(data);
        HandleLog(message);
        var callbackArgs = new OperateCallbackArgs(sendArgs)
            .AppendSuccess();
        Operator.SendCommand(CommandTypes.OperateCallback, callbackArgs);
    }

    private void ReceiveOperateCallback(Command command)
    {

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

    public void UploadFile(string dirName, string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var localPath = Upload.GetFileRepoPath(dirName, fileName);
            if (!File.Exists(localPath))
                File.Copy(filePath, localPath);
            Upload.Upload(dirName, fileName, true);
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
            var fileName = Path.GetFileName(filePath)
; Download.DownLoad(dirName, fileName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void SendMessage(string message)
    {
        var sendArgs = new OperateSendArgs(OperateTypes.Message);
        var data = Encoding.UTF8.GetBytes(message);
        Operator.SendCommandInWaiting(CommandTypes.Operate, sendArgs, data, 0, data.Length);
    }
}
