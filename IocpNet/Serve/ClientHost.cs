using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;
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
        Operator.OnLog += HandleLog;
        Upload.OnLog += HandleLog;
        Download.OnLog += HandleLog;
        HeartBeats.OnLogined += () => OnConnected?.Invoke();
        HeartBeats.OnClosed += () => OnDisconnected?.Invoke();
        Operator.OnOperate += HandleOperate;
        Operator.OnOperateCallback += HandleOperateCallback;
        Upload.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
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

    public void Disconnect()
    {
        Host = null;
        UserInfo = null;
        HeartBeats.Close();
        Operator.Close();
        Upload.Close();
        Download.Close();
        OnDisconnected?.Invoke();
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

    protected override void DoOperate(OperateSendArgs sendArgs)
    {
        Operator.Connect(Host, UserInfo);
        Operator.Operate(sendArgs);
    }

    public void UploadFile(string dirName, string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var localPath = Upload.GetFileRepoPath(dirName, fileName);
        if (!File.Exists(localPath))
        {
            try
            {
                File.Copy(filePath, localPath);
            }
            catch { }
        }
        Upload.Connect(Host, UserInfo);
        Upload.Upload(dirName, fileName, true);
    }

    public void DownloadFile(string dirName, string filePath)
    {
        Download.Connect(Host, UserInfo);
        Download.Download(dirName, Path.GetFileName(filePath), true);
    }
}
