namespace LocalUtilities.IocpNet.Common;

public class DaemonThread
{
    System.Timers.Timer Timer { get; } = new()
    {
        Enabled = false,
        AutoReset = true,
    };

    public DaemonThread(int timeoutMilliseconds, Action processDaemon)
    {
        Timer.Interval = timeoutMilliseconds;
        Timer.Elapsed += (_, _) => processDaemon();
    }

    public void Start()
    {
        Timer.Start();
    }

    public void Stop()
    {
        Timer.Stop();
    }

    public void Dispose()
    {
        Timer.Dispose();
    }
}
