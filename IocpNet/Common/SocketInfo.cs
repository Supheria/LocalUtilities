using System.Net;
using System.Net.Sockets;

namespace LocalUtilities.IocpNet;

public class SocketInfo
{
    public EndPoint? RemoteEndPoint { get; private set; } = null;

    public EndPoint? LocalEndPoint { get; private set; } = null;

    public DateTime ConnectTime { get; private set; } = DateTime.Now;

    public DateTime ActiveTime { get; private set; } = DateTime.Now;

    public DateTime DisconnectTime { get; private set; } = DateTime.Now;

    public bool IsConnect { get; private set; } = false;

    public void Connect(Socket socket)
    {
        RemoteEndPoint = socket.RemoteEndPoint;
        LocalEndPoint = socket.LocalEndPoint;
        ConnectTime = DateTime.Now;
        ActiveTime = DateTime.Now;
        DisconnectTime = DateTime.Now;
        IsConnect = true;
    }

    public void Active()
    {
        ActiveTime = DateTime.Now;
        IsConnect = true;
    }

    public void Disconnect()
    {
        DisconnectTime = DateTime.Now;
        IsConnect = false;
    }
}
