using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;

namespace LocalUtilities.IocpNet.Serve;

public class IocpHost
{
    public event LogHandler? OnLog;

    public event IocpEventHandler<int>? OnConnectionCountChange;

    Socket? Socket { get; set; } = null;

    public bool IsStart { get; private set; } = false;

    ConcurrentDictionary<string, UserHost> UserMap { get; } = [];

    public void Start(int port)
    {
        try
        {
            if (IsStart)
                throw new IocpException(ProtocolCode.HostHasStarted);
            // 使用0.0.0.0作为绑定IP，则本机所有的IPv4地址都将绑定
            var localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
            Socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(localEndPoint);
            Socket.Listen();
            AcceptAsync(null);
            IsStart = true;
            HandleLog("host start");
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void Close()
    {
        try
        {
            if (!IsStart)
                throw new IocpException(ProtocolCode.HostNotStartYet);
            foreach (var user in UserMap.Values)
                user.CloseAll();
            Socket?.Close();
            IsStart = false;
            HandleLog("host close");
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void AcceptAsync(SocketAsyncEventArgs? acceptArgs)
    {
        if (acceptArgs == null)
        {
            acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += (_, args) => ProcessAccept(args);
        }
        else
        {
            acceptArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
        }
        if (Socket is not null && !Socket.AcceptAsync(acceptArgs))
            ProcessAccept(acceptArgs);
    }

    private void ProcessAccept(SocketAsyncEventArgs acceptArgs)
    {
        if (acceptArgs.AcceptSocket is null)
            goto ACCEPT;
        var protocol = new HostProtocol();
        protocol.OnLogined += () =>
        {
            if (protocol.UserInfo?.Name is null || protocol.UserInfo.Name is "")
            {
                protocol.Close();
                return;
            }
            if (UserMap.TryGetValue(protocol.UserInfo.Name, out var user))
                user.Add(protocol);
            else
            {
                user = new();
                user.OnLog += (s) => OnLog?.Invoke(s);
                user.OnClearUp += () => UserMap.TryRemove(user.Name, out _);
                if (!user.Add(protocol))
                    return;
                if (!UserMap.TryAdd(user.Name, user))
                    protocol.Close();
            }
            OnConnectionCountChange?.Invoke(UserMap.Sum(u => u.Value.Count));
        };
        protocol.OnClosed += () =>
        {
            if (protocol.UserInfo?.Name is null || protocol.UserInfo.Name is "" || !UserMap.TryGetValue(protocol.UserInfo.Name, out var user))
                return;
            user.Remove(protocol);
            OnConnectionCountChange?.Invoke(UserMap.Sum(g => g.Value.Count));
        };
        protocol.ProcessAccept(acceptArgs.AcceptSocket);
    ACCEPT:
        if (acceptArgs.SocketError is SocketError.Success)
            AcceptAsync(acceptArgs);
    }

    private void HandleLog(string message)
    {
        // TODO:
        OnLog?.Invoke(message);
    }

    private void HandleException(Exception ex)
    {
        // TODO:
        HandleLog(ex.Message);
    }

    public void BroadcastMessage(string message)
    {
        foreach(var user in  UserMap.Values)
            user.SendMessage(message);
    }
}
