using LocalUtilities.IocpNet.Common.OperateArgs;
using LocalUtilities.IocpNet.Protocol;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Transfer;

partial class Protocol
{
    public event LogHandler? OnLog;

    public event IocpEventHandler? OnLogined;

    public event IocpEventHandler? OnClosed;

    public event IocpEventHandler<string>? OnProcessing;

    public event IocpEventHandler<OperateReceiveArgs>? OnOperate;

    public event IocpEventHandler<OperateCallbackArgs>? OnOperateCallback;

    protected void HandleLog(string log)
    {
        OnLog?.Invoke(GetLog(log));
    }

    protected abstract string GetLog(string log);

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
