namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void RemoveListener(Enum eventType, Callback callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback) - callback;
        OnRemovedListener(eventType);
    }

    public void RemoveListener<T>(Enum eventType, Callback<T> callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T>) - callback;
        OnRemovedListener(eventType);
    }

    public void RemoveListener<T1, T2>(Enum eventType, Callback<T1, T2> callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2>) - callback;
        OnRemovedListener(eventType);
    }

    public void RemoveListener<T1, T2, T3>(Enum eventType, Callback<T1, T2, T3> callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3>) - callback;
        OnRemovedListener(eventType);
    }

    public void RemoveListener<T1, T2, T3, T4>(Enum eventType, Callback<T1, T2, T3, T4> callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3, T4>) - callback;
        OnRemovedListener(eventType);
    }

    public void RemoveListener<T1, T2, T3, T4, T5>(Enum eventType, Callback<T1, T2, T3, T4, T5> callback)
    {
        OnRemovingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3, T4, T5>) - callback;
        OnRemovedListener(eventType);
    }
}
