using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class IocpClient
{
    public event LogHandler? OnLog;

    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public event IocpEventHandler<string>? OnProcessing;

    ClientProtocol HeartBeats { get; } = new(IocpProtocolTypes.HeartBeats);

    ClientProtocol Operator { get; } = new(IocpProtocolTypes.Operator);

    ClientProtocol Upload { get; } = new(IocpProtocolTypes.Upload);

    ClientProtocol Download { get; } = new(IocpProtocolTypes.Download);

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    UserInfo? UserInfo { get; set; } = null;

    ConcurrentDictionary<string, OperateSendArgs> OperateWaitList { get; } = [];

    public IocpClient()
    {
        HeartBeats.OnLog += (s) => OnLog?.Invoke(s);
        HeartBeats.OnLogined += () => OnConnected?.Invoke();
        HeartBeats.OnClosed += () => OnDisconnected?.Invoke();
        Operator.OnLog += (s) => OnLog?.Invoke(s);
        Operator.OnOperate += ProcessOperate;
        Operator.OnOperateCallback += ProcessOperateCallback;
        Upload.OnLog += (s) => OnLog?.Invoke(s);
        Download.OnLog += (s) => OnLog?.Invoke(s);
        Upload.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
        Download.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
    }

    private void ProcessOperate(OperateReceiveArgs args)
    {
        switch (args.Type)
        {
            case OperateTypes.Message:
                OnLog?.Invoke(args.Arg);
                return;
        }
    }

    private void ProcessOperateCallback(OperateCallbackArgs args)
    {
        if (!OperateWaitList.TryGetValue(args.TimeStamp, out var sendArgs))
            return;
        HandleCallbackCode(sendArgs.Type, args.CallbackCode, args.ErrorMessage);
        if (args.CallbackCode is ProtocolCode.Success)
        {
            sendArgs.Waste();
            // TODO: process success
            return;
        }
        sendArgs.Reuse();
        switch (sendArgs.Type)
        {
            case OperateTypes.Message:
                Operator.Operate(sendArgs);
                return;
        }
    }

    public void Connect(string address, int port, string name, string password)
    {
        Host = new(IPAddress.Parse(address), port);
        UserInfo = new(name, password);
        HeartBeats.Connect(Host, UserInfo);
        Operator.Connect(Host, UserInfo);
        Upload.Connect(Host, UserInfo);
        Download.Connect(Host, UserInfo);
    }

    public void Disconnect()
    {
        Host = null;
        UserInfo = null;
        HeartBeats.Close();
        Operator.Close();
        Upload.Close();
        Download.Close();
        OnDisconnected?.Invoke();
    }

    public void Close()
    {
        HeartBeats.Dispose();
        Operator.Dispose();
        Upload.Dispose();
        Download.Dispose();
        Host = null;
        UserInfo = null;
    }

    public void SendMessage(string message)
    {
        var sendArgs = new OperateSendArgs(OperateTypes.Message, message);
        sendArgs.OnWaste += () =>
        {
            OperateWaitList.TryRemove(sendArgs.TimeStamp, out _);
        };
        OperateWaitList.TryAdd(sendArgs.TimeStamp, sendArgs);
        Operator.Operate(sendArgs);
    }

    public void UploadFile(string dirName, string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var localPath = Upload.GetFileRepoPath(dirName, fileName);
        if (!File.Exists(localPath))
        {
            try
            {
                File.Copy(filePath, localPath);
            }
            catch { }
        }
        Upload.Upload(dirName, fileName, true);
    }

    public void DownloadFile(string dirName, string filePath)
    {
        Download.Download(dirName, Path.GetFileName(filePath), true);
    }

    private void HandleLog(string message)
    {
        // TODO:
        OnLog?.Invoke(message);
    }

    private void HandleCallbackCode(OperateTypes operate, ProtocolCode code, string? errorMessage)
    {
        var message = new StringBuilder()
            .Append(operate)
            .Append(SignTable.OpenBracket)
            .Append(code)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.OpenParenthesis)
            .Append(errorMessage)
            .Append(SignTable.CloseParenthesis)
            .ToString();
        HandleLog(message);
    }
}
