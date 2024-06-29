namespace LocalUtilities.IocpNet.Protocol;

public delegate void LogHandler(string log);

public delegate void IocpEventHandler();

public delegate void IocpEventHandler<TArgs>(TArgs args);

public static class DelegateTool
{
    public static void InvokeAsync(this LogHandler onEvent, string log)
    {
        new Task(() => onEvent?.Invoke(log)).Start();
    }

    public static void InvokeAsync(this IocpEventHandler onEvent)
    {
        new Task(() => onEvent?.Invoke()).Start();
    }

    public static void InvokeAsync<TArgs>(this IocpEventHandler<TArgs> onEvent, TArgs args)
    {
        new Task(() => onEvent?.Invoke(args)).Start();
    }
}
