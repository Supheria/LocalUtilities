namespace LocalUtilities.SimpleScript.Parser;

internal class SsParseExceptions(string message) : Exception(message)
{

    internal static string UnknownError(Element element)
    {
        return ($"unknown error at line({element.Line}), column({element.Column})");
    }

    internal static string UnexpectedName(Element element)
    {
        return ($"unexpected name at line({element.Line}), column({element.Column})");
    }

    internal static string UnexpectedOperator(Element element)
    {
        return ($"unexpected operator at line({element.Line}), column({element.Column})");
    }

    internal static string UnexpectedValue(Element element)
    {
        return ($"unexpected value at line({element.Line}), column({element.Column})");
    }

    internal static string UnexpectedArrayType(Element element)
    {
        return ($"unexpected array type at line({element.Line}), column({element.Column})");
    }

    internal static string UnexpectedArraySyntax(Element element)
    {
        return ($"unexpected array syntax at line({element.Line}), column({element.Column})");
    }
}