using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LocalUtilities.IocpNet.Serve;

public class IocpHost
{
    public event LogHandler? OnLog;

    public event IocpEventHandler<int>? OnConnectionCountChange;

    Socket? Socket { get; set; } = null;

    public bool IsStart { get; private set; } = false;

    ConcurrentDictionary<string, ConcurrentDictionary<IocpProtocolTypes, HostProtocol>> UserMap { get; } = [];

    private DaemonThread DaemonThread { get; }

    public IocpHost()
    {
        DaemonThread = new(ConstTabel.TimeoutMilliseconds, ClearBadUser);
    }

    private void ClearBadUser()
    {
        if (!UserMap.TryGetValue("", out var badGroup))
            return;
        foreach (var protocol in badGroup.Values)
        {
            lock (protocol)
                protocol.Close();
        }
    }

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
            DaemonThread.Start();
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
            foreach (var group in UserMap.Values)
            {
                foreach (var protocol in group.Values)
                    protocol.Close();
            }
            Socket?.Close();
            DaemonThread.Stop();
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
        protocol.OnLog += (s) => OnLog?.Invoke(s);
        protocol.OnLogined += () =>
        {
            var name = protocol.UserInfo?.Name ?? "";
            if (UserMap.TryGetValue(name, out var group))
            {
                if (group.TryGetValue(protocol.Type, out var toCheck) && toCheck.TimeStamp != protocol.TimeStamp)
                    protocol.Close();
                else
                    group.TryAdd(protocol.Type, protocol);
            }
            else
            {
                group = new();
                if (!group.TryAdd(protocol.Type, protocol) || !UserMap.TryAdd(name, group))
                    protocol.Close();
            }
            OnConnectionCountChange?.Invoke(UserMap.Sum(g => g.Value.Count));
        };
        protocol.OnClosed += () =>
        {
            var name = protocol.UserInfo?.Name ?? "";
            if (!UserMap.TryGetValue(name, out var group))
                return;
            if (group.TryGetValue(protocol.Type, out var toCheck) && toCheck.TimeStamp == protocol.TimeStamp)
                group.TryRemove(protocol.Type, out _);
            if (group.Count is 0)
                UserMap.TryRemove(name, out _);
            OnConnectionCountChange?.Invoke(UserMap.Sum(g => g.Value.Count));
        };
        protocol.ProcessAccept(acceptArgs.AcceptSocket);
    ACCEPT:
        if (acceptArgs.SocketError is not SocketError.OperationAborted)
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
}
