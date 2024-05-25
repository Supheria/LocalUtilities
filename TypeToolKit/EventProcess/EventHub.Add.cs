namespace LocalUtilities.TypeToolKit.EventProcess;

partial class EventHub
{
    public void AddListener(Enum eventType, Callback callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback) + callback;
    }

    public void AddListener<T>(Enum eventType, Callback<T> callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T>) + callback;
    }

    public void AddListener<T1, T2>(Enum eventType, Callback<T1, T2> callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2>) + callback;
    }

    public void AddListener<T1, T2, T3>(Enum eventType, Callback<T1, T2, T3> callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3>) + callback;
    }

    public void AddListener<T1, T2, T3, T4>(Enum eventType, Callback<T1, T2, T3, T4> callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3, T4>) + callback;
    }

    public void AddListener<T1, T2, T3, T4, T5>(Enum eventType, Callback<T1, T2, T3, T4, T5> callback)
    {
        OnAddingListener(eventType, callback);
        EventMap[eventType] = (EventMap[eventType] as Callback<T1, T2, T3, T4, T5>) + callback;
    }
}
