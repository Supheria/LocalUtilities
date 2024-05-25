using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeToolKit.EventProcess;
public class EventCallbackException(string message) : ArgumentException(message)
{
    public static EventCallbackException CallbackWrongType(Enum eventType, Type existType, Type argsType)
    {
        return new($"{argsType.Name} is wrong callback type for {eventType.ToWholeString}, which should be {existType.Name}");
    }

    public static EventCallbackException CallbackWrongType<T>(Enum eventType)
    {
        return new($"{eventType.ToWholeString()} to invoke meets wrong callback type of {typeof(T).Name}");
    }

    public static EventCallbackException CallbackEmpty(Enum eventType)
    {
        return new($"callback for {eventType.ToWholeString()} is empty");
    }

    public static EventCallbackException EventNotExisted(Enum eventType)
    {
        return new($"{eventType.ToWholeString()} is not a existed event");
    }
}
