namespace LocalUtilities.TypeToolKit.EventProcess;
public class EventCallbackException(string message) : ArgumentException(message)
{
    public static EventCallbackException CallbackWrongType(string eventName, Type existType, Type argsType)
    {
        return new($"{argsType.Name} is wrong callback type for {eventName}, which should be {existType.Name}");
    }

    public static EventCallbackException CallbackWrongType<T>(string eventName)
    {
        return new($"{eventName} to invoke meets wrong callback type of {typeof(T).Name}");
    }

    public static EventCallbackException CallbackEmpty(string eventName)
    {
        return new($"callback for {eventName} is empty");
    }

    public static EventCallbackException EventNotExisted(string eventName)
    {
        return new($"{eventName} is not a existed event");
    }
}
