namespace LocalUtilities.IocpNet.Common;

public interface INetLogger
{
    public NetEventHandler<string>? OnLog { get; set; }

    public string GetLog(string message);
}
