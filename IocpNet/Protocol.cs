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

    DynamicBuffer SendBuffer { get; } = new(ConstTabel.InitialBufferSize);

    bool IsSendingAsync { get; set; } = false;

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
        SendBuffer.Clear();
        IsSendingAsync = false;
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
                if (CommandReceiver.OutOfLimit(packet))
                {
                    ReceiveBuffer.Clear();
                    break;
                }
                if (!CommandReceiver.FullPacket(packet))
                    break;
                var receiver = new CommandReceiver(packet, out var packetLength);
                if (receiver is not null)
                {
                    receiver.OnLog += this.HandleLog;
                    if (receiver.Data.Length > ConstTabel.DataBytesTransferredMax)
                        throw new NetException(ProtocolCode.DataOutLimit);
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

    public void SendCommand(CommandSender sender)
    {
        var packet = sender.GetPacket();
        SendBuffer.WriteData(packet, 0, packet.Length);
        SendAsync();
    }

    private void SendAsync()
    {
        try
        {
            if (IsSendingAsync || Socket is null || SendBuffer.DataCount is 0)
                return;
            IsSendingAsync = true;
            var sendArgs = new SocketAsyncEventArgs();
            sendArgs.SetBuffer(SendBuffer.GetData());
            sendArgs.Completed += (_, args) => ProcessSend(args);
            if (!Socket.SendAsync(sendArgs))
                ProcessSend(sendArgs);
        }
        catch
        {
            Dispose();
        }
    }

    private void ProcessSend(SocketAsyncEventArgs sendArgs)
    {
        SocketInfo.Active();
        IsSendingAsync = false;
        if (sendArgs.SocketError is not SocketError.Success)
            return;
        SendBuffer.Clear(); // 清除已发送的包
        SendAsync();
    }
}
