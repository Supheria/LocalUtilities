﻿using LocalUtilities.IocpNet.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace LocalUtilities.IocpNet.Protocol;

public abstract class IocpProtocol : IDisposable
{
    public event LogHandler? OnLog;

    public event IocpEventHandler? OnLogined;

    public event IocpEventHandler? OnClosed;

    public event IocpEventHandler<string>? OnProcessing;

    public event IocpEventHandler<OperateReceiveArgs>? OnOperate;

    public event IocpEventHandler<OperateCallbackArgs>? OnOperateCallback;

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

    protected abstract DaemonThread DaemonThread { get; }

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
        AutoFile.Close();
        SocketInfo.Disconnect();
        DaemonThread.Stop();
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
            var command = Encoding.UTF8.GetString(buffer, offset, commandLength);
            var commandParser = CommandParser.Parse(command, receiveArgs.BytesTransferred);
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
        var commandLength = WriteU8Buffer(command, out var commandBuffer);
        // 获取总大小(4个字节的包总长度+4个字节的命令长度+命令字节数组的长度+数据的字节数组长度)
        int totalLength = sizeof(int) + sizeof(int) + commandLength + count;
        SendBuffer.StartPacket();
        SendBuffer.DynamicBufferManager.WriteValue(totalLength, false); // 写入总大小
        SendBuffer.DynamicBufferManager.WriteValue(commandLength, false); // 写入命令大小
        SendBuffer.DynamicBufferManager.WriteData(commandBuffer); // 写入命令内容
        SendBuffer.DynamicBufferManager.WriteData(buffer, offset, count); // 写入二进制数据
        SendBuffer.EndPacket();
    }

    protected static int WriteU8Buffer(string? str, [NotNullWhen(true)] out byte[] buffer)
    {
        buffer = [];
        try
        {
            if (str is null)
                return 0;
            buffer = Encoding.UTF8.GetBytes(str);
            return buffer.Length;
        }
        catch
        {
            return 0;
        }
    }

    protected static string ReadU8Buffer(byte[] buffer, int offset, int count)
    {
        try
        {
            return Encoding.UTF8.GetString(buffer, offset, count);
        }
        catch
        {
            return "";
        }
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

    public abstract string GetLog(string message);

    private void HandleLog(string message)
    {
        OnLog?.Invoke(GetLog(message));
    }

    protected void HandleMessage(string message)
    {
        HandleLog(message);
    }

    protected void HandleException(Exception ex)
    {
        HandleLog(ex.Message);
    }

    protected void HandleLogined()
    {
        HandleLog("login");
        OnLogined?.Invoke();
    }

    protected void HandleUploadStart()
    {
        HandleLog("upload file start...");
    }

    protected void HandleDownloadStart()
    {
        HandleLog("download file start...");
    }

    protected void HandleUploading(long fileLength, long position)
    {
        var message = new StringBuilder()
            .Append("uploading")
            .Append(Math.Round(position * 100d / fileLength, 2))
            .Append(SignTable.Percent)
            .ToString();
        OnProcessing?.Invoke(message);
    }

    protected void HandleDownloading(long fileLength, long position)
    {
        var message = new StringBuilder()
            .Append("downloading")
            .Append(Math.Round(position * 100d / fileLength, 2))
            .Append(SignTable.Percent)
            .ToString();
        OnProcessing?.Invoke(message);
    }

    protected void HandleUploaded(string startTime)
    {
        var span = DateTime.Now - startTime.ToDateTime(DateTimeFormat.Data);
        var message = new StringBuilder()
            .Append("upload file success")
            .Append(SignTable.OpenParenthesis)
            .Append(Math.Round(span.TotalMilliseconds, 2))
            .Append("ms")
            .Append(SignTable.CloseParenthesis)
            .ToString();
        HandleLog(message);
        OnProcessing?.Invoke(message);
    }

    protected void HandleDownloaded(string startTime)
    {
        var span = DateTime.Now - startTime.ToDateTime(DateTimeFormat.Data);
        var message = new StringBuilder()
            .Append("download file success")
            .Append(SignTable.OpenParenthesis)
            .Append(Math.Round(span.TotalMilliseconds, 2))
            .Append("ms")
            .Append(SignTable.CloseParenthesis)
            .ToString();
        HandleLog(message);
        OnProcessing?.Invoke(message);
    }

    protected void HandleClosed()
    {
        HandleLog("close");
        OnClosed?.Invoke();
    }

    protected void HandleOperate(OperateReceiveArgs args)
    {
        OnOperate?.Invoke(args);
    }

    protected void HandleOperateCallback(OperateCallbackArgs args)
    {
        OnOperateCallback?.Invoke(args);
    }

    //protected void HandleTestTransferSpeed(int bytesTransferred, TimeSpan span)
    //{
    //    var speed = bytesTransferred * 1000 / span.TotalMilliseconds;
    //    var sb = new StringBuilder();
    //    if (speed > ConstTabel.OneMB)
    //    {
    //        sb.Append(Math.Round(speed / ConstTabel.OneMB, 2))
    //            .Append("MB/s");
    //    }
    //    else if (speed > ConstTabel.OneKB)
    //    {
    //        sb.Append(Math.Round(speed / ConstTabel.OneKB, 2))
    //            .Append("KB/s");
    //    }
    //    else
    //    {
    //        sb.Append(Math.Round(speed, 2))
    //            .Append("KB/s");
    //    }
    //    OnTestTransferSpeed?.InvokeAsync(sb.ToString());
    //}
}
