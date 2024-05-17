namespace LocalUtilities.SimpleScript.Common;

internal class SsFormatException(string message) : Exception(message)
{
    internal static SsFormatException ElementInMultiLines(string str)
    {
        return new($"element cannot contains in multi-lines: {str}");
    }
}
