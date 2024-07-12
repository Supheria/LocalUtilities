using LocalUtilities.IocpNet.Common;
using System.Net.Sockets;

namespace LocalUtilities.IocpNet;

public abstract class Protocol : INetLogger
{
    public event NetEventHandler? OnDisposed;

    public event NetEventHandler<CommandReceiver>? OnReceiveCommand;

    public NetEventHandler<string>? OnLog { get; set; }

    protected Socket? Socket { get; set; } = null;

    public SocketInfo SocketInfo { get; } = new();

    DynamicBuffer ReceiveBuffer { get; } = new(ConstTabel.InitialBufferSize);

    public string GetLog(string message)
    {
        return message;
    }

    public void Dispose()
    {
        try
        {
            Socket?.Shutdown(SocketShutdown.Both);
        }
        catch { }
        Socket?.Close();
        Socket = null;
        ReceiveBuffer.Clear();
        SocketInfo.Disconnect();
        OnDisposed?.Invoke();
    }

    protected void ReceiveAsync()
    {
        var receiveArgs = new SocketAsyncEventArgs();
        receiveArgs.SetBuffer(new byte[ReceiveBuffer.TotolCount], 0, ReceiveBuffer.TotolCount);
        receiveArgs.Completed += (_, args) => ProcessReceive(args);
        try
        {
            if (Socket is not null && !Socket.ReceiveAsync(receiveArgs))
            {
                lock (Socket)
                    ProcessReceive(receiveArgs);
            }
        }
        catch (Exception ex)
        {
            this.HandleException(ex);
            Dispose();
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
    {
        try
        {
            if (Socket is null ||
                receiveArgs.Buffer is null ||
                receiveArgs.BytesTransferred <= 0 ||
                receiveArgs.SocketError is not SocketError.Success)
                throw new NetException(ProtocolCode.SocketClosed);
            SocketInfo.Active();
            ReceiveBuffer.WriteData(receiveArgs.Buffer!, receiveArgs.Offset, receiveArgs.BytesTransferred);
            var packet = ReceiveBuffer.GetData();
            while (packet.Length > sizeof(int))
            {
                if (!CommandReceiver.FullPacket(packet))
                    break;
                var receiver = new CommandReceiver(packet, out var packetLength);
                if (receiver is not null)
                {
                    receiver.OnLog += this.HandleLog;
                    OnReceiveCommand?.Invoke(receiver);
                }
                ReceiveBuffer.RemoveData(packetLength);
                packet = ReceiveBuffer.GetData();
            }
            ReceiveAsync();
            return;
        }
        catch (Exception ex)
        {
            this.HandleException(ex);
            Dispose();
        }
    }

    public void SendAsync(CommandSender sender)
    {
        if (Socket is null)
            return;
        var sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(sender.GetPacket());
        Socket.SendAsync(sendArgs);
    }
}
