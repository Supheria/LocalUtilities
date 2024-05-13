namespace LocalUtilities.SimpleScript.Parser;

internal class SsParseExceptions(string message) : Exception(message)
{

    internal static SsParseExceptions UnknownError(Element element)
    {
        return new($"unknown error at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions UnexpectedName(Element element)
    {
        return new($"unexpected name at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions UnexpectedOperator(Element element)
    {
        return new($"unexpected operator at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions UnexpectedValue(Element element)
    {
        return new($"unexpected value at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions UnexpectedArrayType(Element element)
    {
        return new($"unexpected array type at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions UnexpectedArraySyntax(Element element)
    {
        return new($"unexpected array syntax at line({element.Line}), column({element.Column})");
    }

    internal static SsParseExceptions MultiAssignment(Word name)
    {
        return new($"multi-assignment to {name.Text} at line({name.Line}), column({name.Column})");
    }
}