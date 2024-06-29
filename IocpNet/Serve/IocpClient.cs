using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Serve;

public class IocpClient
{
    ClientProtocol Client { get; } = new();

    public event LogHandler? OnLog;

    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public bool IsConnect => Client.SocketInfo.IsConnect;

    public AutoResetEvent SwitchDone { get; } = new(false);

    public IocpClient()
    {
        Client.OnLog += (s) => OnLog?.Invoke(s);
        Client.OnLogined += () => OnConnected?.Invoke();
        Client.OnClosed += () => OnDisconnected?.Invoke();
    }

    public void Connect(string host, int port, string useName, string password)
    {
        Client.Connect(host, port, useName, password);
    }

    public void Disconnect()
    {
        Client.Close();
    }

    public void SendMessage(string message)
    {
        Client.SendMessage(message);
    }

    public void Upload(string dirName, string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var localPath = Client.GetFileRepoPath(dirName, fileName);
        if (!File.Exists(localPath))
        {
            try
            {
                File.Copy(filePath, localPath);
            }
            catch { }
        }
        Client.Upload(dirName, fileName, true);
    }

    public void Download(string dirName, string filePath)
    {
        Client.Download(dirName, Path.GetFileName(filePath), true);
    }
}
