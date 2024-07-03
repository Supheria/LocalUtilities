using LocalUtilities.IocpNet.Common;
using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Serve;

public abstract class Host
{
    public event LogHandler? OnLog;

    protected UserInfo? UserInfo { get; set; } = null;

    ConcurrentDictionary<string, OperateSendArgs> OperateWaitList { get; } = [];

    private void Operate(OperateSendArgs sendArgs)
    {
        sendArgs.OnLog += HandleLog;
        sendArgs.OnRetry += () => DoOperate(sendArgs);
        sendArgs.OnWasted += () => OperateWaitList.TryRemove(sendArgs.TimeStamp, out _);
        OperateWaitList.TryAdd(sendArgs.TimeStamp, sendArgs);
        DoOperate(sendArgs);
    }

    protected abstract void DoOperate(OperateSendArgs sendArgs);

    protected void HandleOperate(OperateReceiveArgs receiveArgs)
    {
        switch (receiveArgs.Type)
        {
            case OperateTypes.Message:
                HandleLog(receiveArgs.Arg);
                return;
            case OperateTypes.DownloadRequest:

        }
    }

    protected void HandleOperateCallback(OperateCallbackArgs callbackArgs)
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

    protected virtual void HandleDownloadRequest(OperateReceiveArgs args)
    {

    }

    public void SendMessage(string message)
    {
        var sendArgs = new OperateSendArgs(OperateTypes.Message, message);
        Operate(sendArgs);
    }

    protected void HandleLog(string log)
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

    protected void HandleCallbackCode(OperateTypes operate, ProtocolCode code, string? errorMessage)
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
