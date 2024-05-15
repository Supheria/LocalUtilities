using System.IO;

namespace LocalUtilities.SimpleScript.Parser;

internal class SsParseExceptions(string message) : Exception(message)
{
    internal static SsParseExceptions UnexpectedOperator(Token element, string step)
    {
        return new($"step on {step}: unexpected operator {element}");
    }

    internal static SsParseExceptions UnexpectedDelimiter(Token element, string step)
    {
        return new($"step on {step}: unexpected delimiter {element}");
    }

    internal static SsParseExceptions UnexpectedValue(Token element, string step)
    {
        return new($"step on {step}: unexpected value {element})");
    }

    internal static SsParseExceptions MultiAssignment(Word name)
    {
        return new($"multi-assignment to {name}");
    }

    internal static SsParseExceptions CannotFindEntry(string localName)
    {
        return new($"cannot find any entry of {localName}");
    }

    internal static SsParseExceptions CannotFindEntry(string localName, string filePath)
    {
        return new($"cannot find any entry of {localName} in \"{filePath}\"");
    }

    internal static SsParseExceptions CannotOpenFile(string filePath)
    {
        return new($"cannot open file: \"{filePath}\"");
    }
}