using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.IocpNet.Transfer;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace LocalUtilities.IocpNet.Serve;

public class ClientHost
{
    public event LogHandler? OnLog;

    public event IocpEventHandler? OnConnected;

    public event IocpEventHandler? OnDisconnected;

    public event IocpEventHandler<string>? OnProcessing;

    ClientProtocol HeartBeats { get; } = new(ProtocolTypes.HeartBeats);

    ClientProtocol Operator { get; } = new(ProtocolTypes.Operator);

    ClientProtocol Upload { get; } = new(ProtocolTypes.Upload);

    ClientProtocol Download { get; } = new(ProtocolTypes.Download);

    public bool IsConnect => Host is not null;

    IPEndPoint? Host { get; set; } = null;

    UserInfo? UserInfo { get; set; } = null;

    ConcurrentDictionary<string, OperateSendArgs> OperateWaitList { get; } = [];

    public ClientHost()
    {
        HeartBeats.OnLog += HandleLog;
        Operator.OnLog += HandleLog;
        Upload.OnLog += HandleLog;
        Download.OnLog += HandleLog;
        HeartBeats.OnLogined += () => OnConnected?.Invoke();
        HeartBeats.OnClosed += () => OnDisconnected?.Invoke();
        Operator.OnOperate += ProcessOperate;
        Operator.OnOperateCallback += ProcessOperateCallback;
        Upload.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
        Download.OnProcessing += (speed) => OnProcessing?.Invoke(speed);
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

    private void ProcessOperate(OperateReceiveArgs args)
    {
        switch (args.Type)
        {
            case OperateTypes.Message:
                HandleLog(args.Arg);
                return;
        }
    }

    private void ProcessOperateCallback(OperateCallbackArgs callbackArgs)
    {
        if (!OperateWaitList.TryGetValue(callbackArgs.TimeStamp, out var sendArgs))
            return;
        sendArgs.Waste();
        HandleCallbackCode(sendArgs.Type, callbackArgs.CallbackCode, callbackArgs.ErrorMessage);
        if (callbackArgs.CallbackCode is ProtocolCode.Success)
        {
            // TODO: process success
            return;
        }
    }

    private void Operate(OperateSendArgs sendArgs)
    {
        sendArgs.OnLog += HandleLog;
        sendArgs.OnRetry += operate;
        sendArgs.OnWasted += () => OperateWaitList.TryRemove(sendArgs.TimeStamp, out _);
        OperateWaitList.TryAdd(sendArgs.TimeStamp, sendArgs);
        operate();
        void operate()
        {
            Operator.Connect(Host, UserInfo);
            Operator.Operate(sendArgs);
        }
    }

    public void SendMessage(string message)
    {
        var sendArgs = new OperateSendArgs(OperateTypes.Message, message);
        Operate(sendArgs);
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
        Upload.Connect(Host, UserInfo);
        Upload.Upload(dirName, fileName, true);
    }

    public void DownloadFile(string dirName, string filePath)
    {
        Download.Connect(Host, UserInfo);
        Download.Download(dirName, Path.GetFileName(filePath), true);
    }

    private void HandleLog(string log)
    {
        log = new StringBuilder()
            .Append(UserInfo?.Name)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(log)
            .Append(SignTable.Space)
            .Append(SignTable.At)
            .Append(DateTime.Now.ToString(DateTimeFormat.Outlook))
            .ToString();
        OnLog?.Invoke(log);
    }

    private void HandleCallbackCode(OperateTypes operate, ProtocolCode code, string? errorMessage)
    {
        var log = new StringBuilder()
            .Append(SignTable.OpenBracket)
            .Append(code)
            .Append(SignTable.CloseBracket)
            .Append(SignTable.Space)
            .Append(operate)
            .Append(SignTable.Colon)
            .Append(SignTable.Space)
            .Append(errorMessage)
            .ToString();
        HandleLog(log);
    }
}
