using LocalUtilities.IocpNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LocalUtilities.IocpNet.Transfer.Packet;
using LocalUtilities.TypeGeneral;

namespace LocalUtilities.IocpNet.Transfer;

public class ClientProtocol : Protocol
{
    bool IsConnect { get; set; }

    AutoResetEvent ConnectDone { get; } = new(false);

    public bool Connect(IPEndPoint? host)
    {
        Dispose();
        IsConnect = false;
        var connectArgs = new SocketAsyncEventArgs()
        {
            RemoteEndPoint = host,
        };
        connectArgs.Completed += (_, args) => ProcessConnect(args);
        Socket ??= new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if (!Socket.ConnectAsync(connectArgs))
            ProcessConnect(connectArgs);
        ConnectDone.WaitOne(ConstTabel.BlockkMilliseconds);
        return IsConnect;
    }

    private void ProcessConnect(SocketAsyncEventArgs connectArgs)
    {
        if (connectArgs.ConnectSocket is null)
        {
            Socket?.Close();
            Socket?.Dispose();
            return;
        }
        ReceiveAsync();
        SocketInfo.Connect(connectArgs.ConnectSocket);
        IsConnect = true;
        ConnectDone.Set();
    }
}
