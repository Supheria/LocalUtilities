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

    public delegate void LogHandler(string log);

    public delegate void EventHandler();

    public event LogHandler? OnLog;

    public event EventHandler? OnConnected;

    public event EventHandler? OnClosed;

    public bool IsConnect => Client.SocketInfo.IsConnect;

    public AutoResetEvent SwitchDone { get; } = new(false);

    public IocpClient()
    {
        Client.OnUploaded += (p) => OnLog?.Invoke(GetLog(p, "upload file success"));
        Client.OnDownloaded += (p) => OnLog?.Invoke(GetLog(p, "download file success"));
        Client.OnUploading += (p, progress) => OnLog?.Invoke(GetLog(p, $"uploading {progress}"));
        Client.OnDownloading += (p, progress) => OnLog?.Invoke(GetLog(p, $"downloading {progress}"));
        Client.OnMessage += (p, m) => OnLog?.Invoke(GetLog(p, m));
        Client.OnException += (p, ex) => OnLog?.Invoke(GetLog(p, ex.Message));
        Client.OnConnected += (p) => OnLog?.Invoke(GetLog(p, "connect"));
        Client.OnLogined += (p) => OnLog?.Invoke(GetLog(p, "login"));
        Client.OnClosed += (p) => OnLog?.Invoke(GetLog(p, "close"));
        Client.OnConnected += (_) => OnConnected?.Invoke();
        Client.OnClosed += (_) => OnClosed?.Invoke();
    }

    private static string GetLog(IocpProtocol protocol, string message)
    {
        return new StringBuilder()
            .Append(protocol.SocketInfo.LocalEndPoint)
            .Append(SignTable.Open)
            .Append(protocol.UserInfo?.Name)
            .Append(SignTable.Close)
            .Append(SignTable.Mark)
            .Append(SignTable.Space)
            .Append(message)
            .Append(SignTable.At)
            .Append(DateTime.Now.GetFormatString())
            .ToString();
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
