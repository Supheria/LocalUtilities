namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void TryBroadcast(Enum eventType)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback)?.Invoke();
    }

    public void TryBroadcast<T>(Enum eventType, T arg)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback<T>)?.Invoke(arg);
    }

    public void TryBroadcast<T1, T2>(Enum eventType, T1 arg1, T2 arg2)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback<T1, T2>)?.Invoke(arg1, arg2);
    }

    public void TryBroadcast<T1, T2, T3>(Enum eventType, T1 arg1, T2 arg2, T3 arg3)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
    }

    public void TryBroadcast<T1, T2, T3, T4>(Enum eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback<T1, T2, T3, T4>)?.Invoke(arg1, arg2, arg3, arg4);
    }

    public void TryBroadcast<T1, T2, T3, T4, T5>(Enum eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        if (EventMap.TryGetValue(eventType, out var callback))
            (callback as Callback<T1, T2, T3, T4, T5>)?.Invoke(arg1, arg2, arg3, arg4, arg5);
    }
}
