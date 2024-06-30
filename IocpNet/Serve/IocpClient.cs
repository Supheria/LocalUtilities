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

    ClientProtocol HeartBeatsManager { get; } = new();

    ClientProtocol MessageManager { get; } = new();

    ClientProtocol UploadManager { get; } = new();

    ClientProtocol DownloadManager { get; } = new();

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    UserInfo? UserInfo { get; set; } = null;

    public IocpClient()
    {
        HeartBeatsManager.OnLog += (s) => OnLog?.Invoke(s);
        HeartBeatsManager.OnLogined += () => OnConnected?.Invoke();
        HeartBeatsManager.OnClosed += () => OnDisconnected?.Invoke();
        MessageManager.OnLog += (s) => OnLog?.Invoke(s);
        UploadManager.OnLog += (s) => OnLog?.Invoke(s);
        DownloadManager.OnLog += (s) => OnLog?.Invoke(s);
        UploadManager.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
        DownloadManager.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
    }

    public void Connect(string address, int port, string name, string password)
    {
        Host = new(IPAddress.Parse(address), port);
        UserInfo = new(name, password);
        HeartBeatsManager.Connect(Host, UserInfo);
        HeartBeatsManager.HeartBeats();
    }

    public void Disconnected()
    {
        HeartBeatsManager.Close();
        MessageManager.Close();
        UploadManager.Close();
        DownloadManager.Close();
        Host = null;
        UserInfo = null;
        OnDisconnected?.Invoke();
    }

    public void Close()
    {
        HeartBeatsManager.Dispose();
        MessageManager.Dispose();
        UploadManager.Dispose();
        DownloadManager.Dispose();
        Host = null;
        UserInfo = null;
    }

    public void SendMessage(string message)
    {
        MessageManager.Connect(Host, UserInfo);
        MessageManager.SendMessage(message);
    }

    public void Upload(string dirName, string filePath)
    {
        UploadManager.Connect(Host, UserInfo);
        var fileName = Path.GetFileName(filePath);
        var localPath = UploadManager.GetFileRepoPath(dirName, fileName);
        if (!File.Exists(localPath))
        {
            try
            {
                File.Copy(filePath, localPath);
            }
            catch { }
        }
        UploadManager.Upload(dirName, fileName, true);
    }

    public void Download(string dirName, string filePath)
    {
        DownloadManager.Connect(Host, UserInfo);
        DownloadManager.Download(dirName, Path.GetFileName(filePath), true);
    }
}
