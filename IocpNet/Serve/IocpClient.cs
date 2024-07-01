using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using System.Net;

namespace LocalUtilities.IocpNet.Serve;

public class IocpClient
{
    public event LogHandler? OnLog;

    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public event IocpEventHandler<string>? OnProcessing;

    ClientProtocol HeartBeats { get; } = new(IocpProtocolTypes.HeartBeats);

    ClientProtocol Operator { get; } = new(IocpProtocolTypes.Operator);

    ClientProtocol Upload { get; } = new(IocpProtocolTypes.Upload);

    ClientProtocol Download { get; } = new(IocpProtocolTypes.Download);

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    UserInfo? UserInfo { get; set; } = null;

    public IocpClient()
    {
        HeartBeats.OnLog += (s) => OnLog?.Invoke(s);
        HeartBeats.OnLogined += () => OnConnected?.Invoke();
        HeartBeats.OnClosed += () => OnDisconnected?.Invoke();
        Operator.OnLog += (s) => OnLog?.Invoke(s);
        Operator.OnOperate += ProcessOperate;
        Upload.OnLog += (s) => OnLog?.Invoke(s);
        Download.OnLog += (s) => OnLog?.Invoke(s);
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
        HeartBeats.Dispose();
        Operator.Dispose();
        Upload.Dispose();
        Download.Dispose();
        Host = null;
        UserInfo = null;
    }

    public void SendMessage(string message)
    {
        Operator.Operate(OperateTypes.Message, message);
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
        Upload.Upload(dirName, fileName, true);
    }

    public void DownloadFile(string dirName, string filePath)
    {
        Download.Download(dirName, Path.GetFileName(filePath), true);
    }

    private void ProcessOperate(OperateArgs args)
    {
        switch (args.Type)
        {
            case OperateTypes.Message:
                OnLog?.Invoke(args.Args);
                return;
        }
    }
}
