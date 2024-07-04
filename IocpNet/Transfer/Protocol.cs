using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Transfer;

public abstract partial class Protocol : IDisposable
{
    protected Socket? Socket { get; set; } = null;

    public SocketInfo SocketInfo { get; } = new();

    protected bool IsLogin { get; set; } = false;

    public UserInfo? UserInfo { get; protected set; } = new();

    public bool UseNetByteOrder { get; set; } = false;

    protected DynamicBufferManager ReceiveBuffer { get; } = new(ConstTabel.InitialBufferSize);

    protected AsyncSendBufferManager SendBuffer { get; } = new(ConstTabel.InitialBufferSize);

    protected bool IsSendingAsync { get; set; } = false;

    protected string RepoPath { get; set; } = "repo";

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
        AutoFile.DisposeFileStream();
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
        catch
        {
            Close();
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
    {
        if (Socket is null ||
            receiveArgs.Buffer is null ||
            receiveArgs.BytesTransferred <= 0 ||
            receiveArgs.SocketError is not SocketError.Success)
            goto CLOSE;
        try
        {
            SocketInfo.Active();
            ReceiveBuffer.WriteData(receiveArgs.Buffer!, receiveArgs.Offset, receiveArgs.BytesTransferred);
            // 按照长度分包
            // 小于四个字节表示包头未完全接收，继续接收
            while (ReceiveBuffer.DataCount > sizeof(int))
            {
                var buffer = ReceiveBuffer.GetData();
                var packetLength = BitConverter.ToInt32(buffer, 0);
                if (UseNetByteOrder)
                    packetLength = IPAddress.NetworkToHostOrder(packetLength);
                // 最大Buffer异常保护
                // buffer = [totol legth] + [command length] + [command] + [data]
                var offset = sizeof(int); // totol length
                var commandLength = BitConverter.ToInt32(buffer, offset); //取出命令长度
                offset += sizeof(int); // command length
                var bytesMax = ConstTabel.DataBytesTransferredMax + commandLength + offset;
                if (packetLength > bytesMax || ReceiveBuffer.DataCount > bytesMax)
                    goto CLOSE;
                // 收到的数据没有达到包长度，继续接收
                if (ReceiveBuffer.DataCount < packetLength)
                    goto RECEIVE;
                var command = new Command().ParseSs(buffer, offset, commandLength);
                offset += commandLength;
                // 处理命令,offset + sizeof(int) + commandLen后面的为数据，数据的长度为count - sizeof(int) - sizeof(int) - length，注意是包的总长度－包长度所占的字节（sizeof(int)）－ 命令长度所占的字节（sizeof(int)） - 命令的长度
                ProcessCommand(command, buffer, offset, packetLength - offset);
                ReceiveBuffer.RemoveData(packetLength);
            }
        RECEIVE:
            ReceiveAsync();
            return;
        }
        catch
        {
            goto CLOSE;
        }
    CLOSE:
        Close();
        return;
    }

    public void SendAsync()
    {
        if (IsSendingAsync || Socket is null || !SendBuffer.GetFirstPacket(out var packetOffset, out var packetCount))
            return;
        IsSendingAsync = true;
        var sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(SendBuffer.DynamicBufferManager.GetData(), packetOffset, packetCount);
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
        SendBuffer.ClearFirstPacket(); // 清除已发送的包
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
