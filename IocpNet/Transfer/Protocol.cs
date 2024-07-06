using LocalUtilities.IocpNet.Common;
using System.Net.Sockets;

namespace LocalUtilities.IocpNet.Transfer;

public abstract partial class Protocol : IDisposable
{
    protected Socket? Socket { get; set; } = null;

    public SocketInfo SocketInfo { get; } = new();

    protected bool IsLogin { get; set; } = false;

    public UserInfo? UserInfo { get; protected set; } = new();

    protected DynamicBufferManager ReceiveBuffer { get; } = new(ConstTabel.InitialBufferSize);

    protected AsyncSendBufferManager SendBuffer { get; } = new(ConstTabel.InitialBufferSize);

    protected bool IsSendingAsync { get; set; } = false;

    //protected string RepoPath { get; set; } = "repo";
    protected abstract string RepoPath { get; set; }

    protected AutoDisposeFileStream AutoFile { get; } = new();

    protected DaemonThread? DaemonThread { get; init; }

    public void Close()
    {
        HandleClosed();
        Dispose();
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
        SendBuffer.ClearAllPacket();
        IsSendingAsync = false;
        IsLogin = false;
        AutoFile.Dispose();
        SocketInfo.Disconnect();
        DaemonThread?.Stop();
        GC.SuppressFinalize(this);
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
            HandleException(ex);
            Close();
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
                throw new IocpException(ProtocolCode.SocketClosed);
            SocketInfo.Active();
            ReceiveBuffer.WriteData(receiveArgs.Buffer!, receiveArgs.Offset, receiveArgs.BytesTransferred);
            var packet = ReceiveBuffer.GetData();
            while (Command.FullPacket(packet))
            {
                var command = new Command(packet);
                if (command.Data.Length > ConstTabel.DataBytesTransferredMax)
                    throw new IocpException(ProtocolCode.DataOutLimit);
                ProcessCommand(command);
                ReceiveBuffer.RemoveData(command.PacketLength);
                packet = ReceiveBuffer.GetData();
            }
            ReceiveAsync();
            return;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            Close();
        }
    }

    public void SendAsync()
    {
        if (IsSendingAsync || Socket is null || !SendBuffer.GetFirstPacket(out var packet))
            return;
        IsSendingAsync = true;
        var sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(packet);
        sendArgs.Completed += (_, args) => ProcessSend(args);
        try
        {
            if (!Socket.SendAsync(sendArgs))
                new Task(() => ProcessSend(sendArgs)).Start();
        }
        catch
        {
            Close();
        }
    }

    private void ProcessSend(SocketAsyncEventArgs sendArgs)
    {
        SocketInfo.Active();
        IsSendingAsync = false;
        if (sendArgs.SocketError is not SocketError.Success)
            return;
        //SendBuffer.ClearFirstPacket(); // 清除已发送的包
        SendAsync();
    }



    public string GetFileRepoPath(string dirName, string fileName)
    {
        var dir = Path.Combine(RepoPath, dirName);
        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        return Path.Combine(dir, fileName);
    }
}
