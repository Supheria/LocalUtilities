namespace LocalUtilities.TypeToolKit.Convert;

internal class TypeConvertException(string message) : Exception(message)
{
    internal static TypeConvertException CannotConvertStringTo<T>()
    {
        return new($"cannot convert string to {typeof(T)}");
    }

    internal static TypeConvertException CannotConvertStringArrayTo<T>()
    {
        return new($"cannot convert string array to {typeof(T)}");
    }
}
