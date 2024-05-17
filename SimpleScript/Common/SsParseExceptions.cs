using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Common;

internal class SsParseExceptions(string message) : Exception(message)
{
    internal static SsParseExceptions UnexpectedOperator(Token token, string step)
    {
        return new($"step on {step}: unexpected operator {token}");
    }

    internal static SsParseExceptions UnexpectedDelimiter(Token token, string step)
    {
        return new($"step on {step}: unexpected delimiter {token}");
    }

    internal static SsParseExceptions UnexpectedValue(Token token, string step)
    {
        return new($"step on {step}: unexpected value {token})");
    }

    internal static SsParseExceptions MultiAssignment(string name)
    {
        return new($"multi-assignment to {name}");
    }

    internal static SsParseExceptions CannotFindEntry(string localName)
    {
        return new($"cannot find any entry of {localName}");
    }

    internal static SsParseExceptions CannotOpenFile(string filePath)
    {
        return new($"cannot open file: \"{filePath}\"");
    }

    public static SsParseExceptions WrongObjectEntry(string localName)
    {
        return new($"not a proper object entry of {localName}");
    }

    internal static SsParseExceptions WrongArrayEntry(string localName)
    {
        return new($"not a proper array entry of {localName}");
    }
}
