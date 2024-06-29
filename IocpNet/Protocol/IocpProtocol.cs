using LocalUtilities.IocpNet.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalUtilities.IocpNet.Protocol;

public abstract class IocpProtocol : IDisposable
{
    protected Socket? Socket { get; set; } = null;

    public SocketInfo SocketInfo { get; } = new();

    protected bool IsLogin { get; set; } = false;

    public UserInfo? UserInfo { get; protected set; } = new();

    public bool UseNetByteOrder { get; set; } = false;

    protected DynamicBufferManager ReceiveBuffer { get; } = new(ConstTabel.InitBufferSize);

    protected AsyncSendBufferManager SendBuffer { get; } = new(ConstTabel.InitBufferSize);

    protected bool IsSendingAsync { get; set; } = false;

    protected string RepoPath { get; set; } = "repo";

    protected Dictionary<string, AutoDisposeFileStream> FileReaders { get; } = [];

    protected Dictionary<string, AutoDisposeFileStream> FileWriters { get; } = [];

    public event IocpEventHandler? OnClosed;

    public event IocpEventHandler<Exception>? OnException;

    public void Close() => Dispose();

    public void Dispose()
    {
        try
        {
            Socket?.Shutdown(SocketShutdown.Both);
        }
        catch (Exception ex)
        {
            //Program.Logger.ErrorFormat("CloseClientSocket Disconnect client {0} error, message: {1}", socketInfo, ex.Message);
        }
        Socket?.Close();
        Socket = null;
        ReceiveBuffer.Clear();
        SendBuffer.ClearAllPacket();
        IsSendingAsync = false;
        IsLogin = false;
        SocketInfo.Disconnect();
        OnClosed?.InvokeAsync(this);
        GC.SuppressFinalize(this);
    }

    public void ReceiveAsync()
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
            var bufferMax = ConstTabel.TransferBufferMax + commandLength + offset;
            if (packetLength > bufferMax || ReceiveBuffer.DataCount > bufferMax)
                goto CLOSE;
            // 收到的数据没有达到包长度，继续接收
            if (ReceiveBuffer.DataCount < packetLength)
                goto RECEIVE;
            var command = Encoding.UTF8.GetString(buffer, offset, commandLength);
            var commandParser = CommandParser.Parse(command);
            offset += commandLength;
            // 处理命令,offset + sizeof(int) + commandLen后面的为数据，数据的长度为count - sizeof(int) - sizeof(int) - length，注意是包的总长度－包长度所占的字节（sizeof(int)）－ 命令长度所占的字节（sizeof(int)） - 命令的长度
            ProcessCommand(commandParser, buffer, offset, packetLength - offset);
            ReceiveBuffer.RemoveData(packetLength);
        }
    RECEIVE:
        ReceiveAsync();
        return;
    CLOSE:
        Close();
        return;
    }

    protected abstract void ProcessCommand(CommandParser commandParser, byte[] buffer, int offset, int count);

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

    protected void WriteCommand(CommandComposer commandComposer)
    {
        WriteCommand(commandComposer, [], 0, 0);
    }

    protected void WriteCommand(CommandComposer commandComposer, byte[] buffer, int offset, int count)
    {
        // 获取命令
        var command = commandComposer.GetCommand();
        // 获取命令的字节数组
        var commandBuffer = Encoding.UTF8.GetBytes(command);
        // 获取总大小(4个字节的包总长度+4个字节的命令长度+命令字节数组的长度+数据的字节数组长度)
        int totalLength = sizeof(int) + sizeof(int) + commandBuffer.Length + count;
        SendBuffer.StartPacket();
        SendBuffer.DynamicBufferManager.WriteValue(totalLength, false); // 写入总大小
        SendBuffer.DynamicBufferManager.WriteValue(commandBuffer.Length, false); // 写入命令大小
        SendBuffer.DynamicBufferManager.WriteData(commandBuffer); // 写入命令内容
        SendBuffer.DynamicBufferManager.WriteData(buffer, offset, count); // 写入二进制数据
        SendBuffer.EndPacket();
    }

    public abstract void SendMessage(string message);

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

    protected void HandleException(Exception ex)
    {
        OnException?.InvokeAsync(this, ex);
    }
}
