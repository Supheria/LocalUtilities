namespace LocalUtilities.DelegateUtilities;

public static class DelegateTool
{
    public static T? RemoveAllInvocations<T>(this T? source) where T : Delegate
    {
        return Delegate.RemoveAll(source, source) as T;
    }
}
