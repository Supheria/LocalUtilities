namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void Broadcast(Enum eventType)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))();
    }

    public void Broadcast<T>(Enum eventType, T arg)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback<T> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))(arg);
    }

    public void Broadcast<T1, T2>(Enum eventType, T1 arg1, T2 arg2)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback<T1, T2> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))(arg1, arg2);
    }

    public void Broadcast<T1, T2, T3>(Enum eventType, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback<T1, T2, T3> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))(arg1, arg2, arg3);
    }

    public void Broadcast<T1, T2, T3, T4>(Enum eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback<T1, T2, T3, T4> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))(arg1, arg2, arg3, arg4);
    }

    public void Broadcast<T1, T2, T3, T4, T5>(Enum eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        if (!EventMap.TryGetValue(eventType, out var callback))
            throw EventCallbackException.EventNotExisted(eventType);
        (callback as Callback<T1, T2, T3, T4, T5> ?? throw EventCallbackException.CallbackWrongType<Callback>(eventType))(arg1, arg2, arg3, arg4, arg5);
    }
}
