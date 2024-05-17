namespace LocalUtilities.TypeGeneral.Convert;

public class TypeConvertException(string message) : Exception(message)
{
    public static TypeConvertException CannotConvertStringTo<T>()
    {
        return new($"cannot convert string to {typeof(T)}");
    }

    public static TypeConvertException CannotConvertStringArrayTo<T>()
    {
        return new($"cannot convert string array to {typeof(T)}");
    }
}
