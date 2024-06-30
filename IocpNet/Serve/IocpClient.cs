using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Serve;

public class IocpClient
{
    ClientProtocol MessageManager { get; } = new();

    ClientProtocol UploadManager { get; } = new();

    ClientProtocol DownloadManager { get; } = new();

    public event LogHandler? OnLog;

    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    UserInfo? UserInfo { get; set; } = null;

    public IocpClient()
    {
        MessageManager.OnLog += (s) => OnLog?.Invoke(s);
        UploadManager.OnLog += (s) => OnLog?.Invoke(s);
        DownloadManager.OnLog += (s) => OnLog?.Invoke(s);
    }

    public void Connect(string address, int port, string name, string password)
    {
        Host = new(IPAddress.Parse(address), port);
        UserInfo = new(name, password);
        OnConnected?.Invoke();
    }

    public void Disconnected()
    {
        MessageManager.Close();
        UploadManager.Close();
        DownloadManager.Close();
        Host = null;
        UserInfo = null;
        OnDisconnected?.Invoke();
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
