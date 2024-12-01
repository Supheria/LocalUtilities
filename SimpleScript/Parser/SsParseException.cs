namespace LocalUtilities.SimpleScript;

internal class SsParseException(string message) : Exception(message)
{
    internal static SsParseException UnexpectedOperator(Token token, string step)
    {
        return new($"step on {step}: unexpected operator {token}");
    }

    internal static SsParseException UnexpectedOperator(Word word, string step)
    {
        return new($"step on {step}: unexpected operator {word}");
    }

    internal static SsParseException UnexpectedDelimiter(Token token, string step)
    {
        return new($"step on {step}: unexpected delimiter {token}");
    }

    internal static SsParseException UnexpectedValue(Token token, string step)
    {
        return new($"step on {step}: unexpected value {token})");
    }

    internal static SsParseException NullProperty()
    {
        return new("property to append cannot be null");
    }

    internal static SsParseException LevelDismatch()
    {
        return new("level dismatched of Appending in Scope");
    }

    internal static SsParseException MultiAssignment(string name)
    {
        return new($"multi-assignment to {name}");
    }

    internal static SsParseException CannotFindEntry(string localName)
    {
        return new($"cannot find any entry of {localName}");
    }

    internal static SsParseException CannotOpenFile(string filePath)
    {
        return new($"cannot open file: \"{filePath}\"");
    }

    public static SsParseException WrongObjectEntry(string localName)
    {
        return new($"not a proper object entry of {localName}");
    }

    internal static SsParseException WrongArrayEntry(string localName)
    {
        return new($"not a proper array entry of {localName}");
    }
}
