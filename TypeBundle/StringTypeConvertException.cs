namespace LocalUtilities.TypeBundle;

internal class StringTypeConvertException(Type type) : Exception($"cannot convert string to {type}")
{
}
