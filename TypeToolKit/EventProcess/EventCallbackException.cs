using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeToolKit.EventProcess;
public class EventCallbackException(string message) : ArgumentException(message)
{
    public static EventCallbackException CallbackWrongType(Enum eventType, Delegate exist, Delegate callback)
    {
        return new($"{callback.Target} callbacks of {callback.GetType()} is wrong type for event {eventType.ToWholeString()}, which type should be {exist.GetType} from {exist.GetType()}");
    }

    public static EventCallbackException CallbackWrongType(Enum eventType, Delegate? callback)
    {
        return new($"{callback?.Target ?? "unknown_object"} callbacks of {callback?.GetType().Name ?? "unknown_object"} is wrong type for event {eventType.ToWholeString()}");
    }

    public static EventCallbackException CallbackEmpty(Enum eventType, Delegate callback)
    {
        return new($"{callback.Target} callbacks for event {eventType.ToWholeString()} is empty");
    }

    public static EventCallbackException EventNotExisted(Enum eventType)
    {
        return new($"{eventType.ToWholeString()} is not a existed event");
    }
}
