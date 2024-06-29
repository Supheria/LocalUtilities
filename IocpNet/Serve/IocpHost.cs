using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using System.Net;
using System.Net.Sockets;

namespace LocalUtilities.IocpNet.Serve;

public class IocpHost
{
    Socket? Socket { get; set; } = null;

    public bool IsStart { get; private set; } = false;

    int ParallelCountMax { get; }

    // TODO: use another way to limit paralle number

    HostProtocolPool ProtocolPool { get; }

    public HostProtocolList ProtocolList { get; } = [];

    private DaemonThread DaemonThread { get; }

    public event LogHandler? OnLog;

    public event IocpEventHandler<int>? OnParallelRemainChange;

    public IocpHost(int parallelCountMax)
    {
        ParallelCountMax = parallelCountMax;
        ProtocolPool = new(parallelCountMax);
        DaemonThread = new(ProcessDaemon);
        for (int i = 0; i < ParallelCountMax; i++)
        {
            var protocol = new HostProtocol();
            protocol.OnLog += (s) => OnLog?.Invoke(s);
            protocol.OnClosed += () =>
            {
                ProtocolPool.Push(protocol);
                ProtocolList.Remove(protocol);
                OnParallelRemainChange?.Invoke(ProtocolPool.Count);
            };
            ProtocolPool.Push(protocol);
        }
    }

    /// <summary>
    /// 守护线程
    /// </summary>
    private void ProcessDaemon()
    {
        ProtocolList.CopyTo(out var userTokens);
        var timeout = ConstTabel.TimeoutMilliseconds;
        foreach (var protocol in userTokens)
        {
            var span = DateTime.Now - protocol.SocketInfo.ActiveTime;
            if (span.TotalMilliseconds > timeout)
            {
                lock (protocol)
                    protocol.Close();
            }
        }
    }

    public void Start(int port)
    {
        if (IsStart)
        {
            //Program.Logger.InfoFormat("server {0} has started ", localEndPoint.ToString());
            return;
        }
        // 使用0.0.0.0作为绑定IP，则本机所有的IPv4地址都将绑定
        var localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
        Socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket.Bind(localEndPoint);
        Socket.Listen(ParallelCountMax);
        //ServerInstance.Logger.InfoFormat("Start listen socket {0} success", localEndPoint.ToString());
        //for (int i = 0; i < 64; i++) //不能循环投递多次AcceptAsync，会造成只接收8000连接后不接收连接了
        AcceptAsync(null);
        DaemonThread.Start();
        IsStart = true;
    }

    public void Stop()
    {
        if (!IsStart)
        {
            //ServerInstance.Logger.Info("server {0} has not started yet", localEndPoint.ToString());
            return;
        }
        ProtocolList.CopyTo(out var userTokens);
        foreach (var protocol in userTokens)//双向关闭已存在的连接
            protocol.Close();
        ProtocolList.Clear();
        Socket?.Close();
        DaemonThread.Stop();
        IsStart = false;
        //ServerInstance.Logger.Info("Server is Stoped");
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
        var protocol = ProtocolPool.Pop();
        if (!protocol.ProcessAccept(acceptArgs.AcceptSocket))
        {
            ProtocolPool.Push(protocol);
            return;
        }
        ProtocolList.Add(protocol);
        OnParallelRemainChange?.Invoke(ProtocolPool.Count);
        protocol.ReceiveAsync();
        if (acceptArgs.SocketError is not SocketError.OperationAborted)
            AcceptAsync(acceptArgs); //把当前异步事件释放，等待下次连接
    }
}
