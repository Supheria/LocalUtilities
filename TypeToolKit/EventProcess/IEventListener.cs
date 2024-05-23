namespace LocalUtilities.TypeToolKit.EventProcess;

public interface IEventListener
{
    void HandleEvent(int eventId, IEventArgument argument);
}
