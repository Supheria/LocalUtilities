using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class Server
{
    public event LogHandler? OnLog;

    public event IocpEventHandler<int>? OnConnectionCountChange;

    Socket? Socket { get; set; } = null;

    public bool IsStart { get; private set; } = false;

    ConcurrentDictionary<string, ServerHost> UserMap { get; } = [];

    public void Start(int port)
    {
        try
        {
            if (IsStart)
                throw new IocpException(ProtocolCode.ServerHasStarted);
            // 使用0.0.0.0作为绑定IP，则本机所有的IPv4地址都将绑定
            var localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
            Socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(localEndPoint);
            Socket.Listen();
            AcceptAsync(null);
            IsStart = true;
            HandleLog("start");
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
                throw new IocpException(ProtocolCode.ServerNotStartYet);
            foreach (var user in UserMap.Values)
                user.CloseAll();
            Socket?.Close();
            IsStart = false;
            HandleLog("close");
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
        var protocol = new ServerProtocol();
        protocol.OnLogined += () => AddProtocol(protocol);
        protocol.OnClosed += () => RemoveProtocol(protocol);
        protocol.ProcessAccept(acceptArgs.AcceptSocket);
    ACCEPT:
        if (acceptArgs.SocketError is SocketError.Success)
            AcceptAsync(acceptArgs);
    }

    private void AddProtocol(ServerProtocol protocol)
    {
        if (protocol.UserInfo is null || protocol.UserInfo.Name is "")
        {
            protocol.Close();
            return;
        }
        if (UserMap.TryGetValue(protocol.UserInfo.Name, out var user))
        {
            user.Add(protocol);
            HandleUpdateConnection();
            return;
        }
        user = new();
        user.OnLog += HandleLog;
        user.OnOperate += HandleOperate;
        user.OnClearUp += () =>
        {
            UserMap.TryRemove(user.Name, out _);
            BroadcastUserList();
        };
        if (!user.Add(protocol))
            return;
        if (!UserMap.TryAdd(user.Name, user))
            protocol.Close();
        HandleUpdateConnection();
    }

    private void RemoveProtocol(ServerProtocol protocol)
    {
        if (protocol.UserInfo is null || protocol.UserInfo.Name is "" || !UserMap.TryGetValue(protocol.UserInfo.Name, out var user))
            return;
        user.Remove(protocol);
        HandleUpdateConnection();
    }

    private void HandleOperate(Command command)
    {
        try
        {
            var receiver = command.GetArgs(ProtocolKey.Receiver);
            if (!UserMap.TryGetValue(receiver, out var user))
                throw new IocpException(ProtocolCode.UserNotExist);
            user.DoOperate(command);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    public void HandleLog(string log)
    {
        log = new StringBuilder()
            .Append(SignTable.OpenParenthesis)
            .Append("host")
            .Append(SignTable.CloseParenthesis)
            .Append(SignTable.Space)
            .Append(log)
            .Append(SignTable.Space)
            .Append(SignTable.At)
            .Append(DateTime.Now.ToString(DateTimeFormat.Outlook))
            .ToString();
        OnLog?.Invoke(log);
    }

    private void HandleException(Exception ex)
    {
        // TODO:
        HandleLog(ex.Message);
    }

    public void BroadcastMessage(string message)
    {
        foreach (var user in UserMap.Values)
            user.SendMessage(message);
    }

    public void BroadcastUserList()
    {
        foreach (var user in UserMap.Values)
            user.UpdateUserList(UserMap.Keys.ToArray());
    }

    public void HandleUpdateConnection()
    {
        OnConnectionCountChange?.Invoke(UserMap.Sum(g => g.Value.Count));
        BroadcastUserList();
    }
}
