namespace LocalUtilities.StringUtilities;

internal class StringTypeConvertException(Type type) : Exception($"cannot convert string to {type}")
{
}
