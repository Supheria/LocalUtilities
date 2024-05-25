namespace LocalUtilities.TypeToolKit.EventProcess;

public partial class EventHub
{
    Dictionary<Enum, Delegate?> EventMap { get; } = [];

    private void OnAddingListener(Enum eventType, Delegate callback)
    {
        if (!EventMap.TryGetValue(eventType, out var exist))
            EventMap.Add(eventType, null);
        if (exist is not null && exist.GetType() != callback.GetType())
            throw EventCallbackException.CallbackWrongType(eventType, exist.GetType(), callback.GetType());
    }

    private void OnRemovingListener(Enum eventType, Delegate callback)
    {
        if (!EventMap.TryGetValue(eventType, out var exist))
            throw EventCallbackException.EventNotExisted(eventType);
        if (exist is null)
            throw EventCallbackException.CallbackEmpty(eventType);
        if (exist.GetType() != callback.GetType())
            throw EventCallbackException.CallbackWrongType(eventType, exist.GetType(), callback.GetType());
    }

    private void OnRemovedListener(Enum eventType)
    {
        if (EventMap[eventType] is null)
            EventMap.Remove(eventType);
    }

    public void ClearListener(Enum eventType)
    {
        EventMap.Remove(eventType);
    }

    public List<KeyValuePair<Enum, Delegate?>> GetEventList()
    {
        return EventMap.ToList();
    }
}
