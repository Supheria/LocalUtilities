namespace LocalUtilities.TypeToolKit.EventProcess;

public class EventArgument<T>(T value) : IEventArgument
{
    T Value { get; set; } = value;

    public T GetValue()
    {
        return Value;
    }
}
