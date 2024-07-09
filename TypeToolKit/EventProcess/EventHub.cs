using System.Collections.Concurrent;

namespace LocalUtilities.TypeToolKit.EventProcess;

public partial class EventHub
{
    ConcurrentDictionary<Enum, Delegate> EventMap { get; } = [];

    public bool TryAddListener(Enum eventType, Callback callback)
    {
        try
        {
            var success = EventMap.TryAdd(eventType, callback);
            if (!EventMap.TryGetValue(eventType, out var exist))
                return false;
            if (success)
                return true;
            EventMap[eventType] = (Callback)exist + callback;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryAddListener<TArgs>(Enum eventType, Callback<TArgs> callback)
    {
        try
        {
            var success = EventMap.TryAdd(eventType, callback);
            if (!EventMap.TryGetValue(eventType, out var exist))
                return false;
            if (success)
                return true;
            EventMap[eventType] = (Callback<TArgs>)exist + callback;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryRemoveListener(Enum eventType, Callback callback)
    {
        try
        {
            if (!EventMap.TryGetValue(eventType, out var exist))
                return false;
            exist = (Callback)exist - callback;
            if (exist is null)
                return EventMap.TryRemove(eventType, out _);
            EventMap[eventType] = exist;
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public bool TryRemoveListener<TArgs>(Enum eventType, Callback<TArgs> callback)
    {
        try
        {
            if (!EventMap.TryGetValue(eventType, out var exist))
                return false;
            exist = (Callback<TArgs>)exist - callback;
            if (exist is null)
                return EventMap.TryRemove(eventType, out _);
            EventMap[eventType] = exist;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryBroadcast(Enum eventType)
    {
        try
        {
            if (!EventMap.TryGetValue(eventType, out var callback))
                return false;
            callback.DynamicInvoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryBroadcast<TArgs>(Enum eventType, TArgs args)
    {
        try
        {
            if (!EventMap.TryGetValue(eventType, out var callback))
                return false;
            callback.DynamicInvoke(args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearListener(Enum eventType)
    {
        EventMap.TryRemove(eventType, out _);
    }

    public List<KeyValuePair<Enum, Delegate>> GetEventList()
    {
        return EventMap.ToList();
    }
}
