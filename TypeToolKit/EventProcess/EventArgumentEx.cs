namespace LocalUtilities.TypeToolKit.EventProcess;

public static class EventArgumentEx
{
    public static T? GetValue<T>(this IEventArgument argument)
    {
        if (argument is EventArgument<T> arg)
            return arg.GetValue();
        return default;
    }
}
