namespace LocalUtilities.TypeToolKit.EventProcess;

public partial class EventHub
{
    Dictionary<string, Delegate?> EventMap { get; } = [];

    private void OnAddingListener(string eventName, Delegate callback)
    {
        if (!EventMap.TryGetValue(eventName, out var exist))
            EventMap.Add(eventName, null);
        if (exist is not null && exist.GetType() != callback.GetType())
            throw EventCallbackException.CallbackWrongType(eventName, exist.GetType(), callback.GetType());
    }

    private void OnRemovingListener(string eventName, Delegate callback)
    {
        if (!EventMap.TryGetValue(eventName, out var exist))
            throw EventCallbackException.EventNotExisted(eventName);
        if (exist is null)
            throw EventCallbackException.CallbackEmpty(eventName);
        if (exist.GetType() != callback.GetType())
            throw EventCallbackException.CallbackWrongType(eventName, exist.GetType(), callback.GetType());
    }

    private void OnRemovedListener(string eventName)
    {
        if (EventMap[eventName] is null)
            EventMap.Remove(eventName);
    }
}
