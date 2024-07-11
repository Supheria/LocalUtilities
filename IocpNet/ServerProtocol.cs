using System.Net.Sockets;

namespace LocalUtilities.IocpNet;

public class ServerProtocol : Protocol
{
    public void Accept(Socket socket)
    {
        if (Socket is not null)
            return;
        Socket = socket;
        SocketInfo.Connect(socket);
        ReceiveAsync();
    }
}
