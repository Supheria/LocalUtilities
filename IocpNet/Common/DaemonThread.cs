namespace LocalUtilities.IocpNet.Common;

public class DaemonThread
{
    System.Timers.Timer Timer { get; set; } 

    public DaemonThread(int timeoutMilliseconds, Action processDaemon)
    {
        Timer = new()
        {
            Enabled = false,
            AutoReset = true,
            Interval = timeoutMilliseconds,
        };
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
        Timer.Stop();
        Timer.Dispose();
    }
}
