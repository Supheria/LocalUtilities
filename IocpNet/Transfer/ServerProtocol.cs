using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Transfer;

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
