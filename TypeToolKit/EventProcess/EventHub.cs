namespace LocalUtilities.TypeToolKit.EventProcess;

public class EventHub
{
    Dictionary<int, List<IEventListener>> EventMap { get; } = [];

    public void AddListener(int eventId, IEventListener listener)
    {
        if (EventMap.TryGetValue(eventId, out var list))
            list.Add(listener);
        else
            EventMap[eventId] = [listener];
    }

    public void RemoveListener(int eventId, IEventListener listener)
    {
        if (EventMap.TryGetValue(eventId, out var list))
            list.Remove(listener);
    }

    public void Dispatch(int eventId, IEventArgument argument)
    {
        if (EventMap.TryGetValue(eventId, out var list)) 
        {
            foreach (var listener in list)
                listener.HandleEvent(eventId, argument);
        }
    }
}