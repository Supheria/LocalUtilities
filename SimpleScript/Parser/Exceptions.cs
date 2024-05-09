using System.Text;

namespace LocalUtilities.SimpleScript.Parser;

internal class Exceptions
{
    internal StringBuilder Log { get; set; } = new();

    internal string FilePath
    {
        get => _filePath;
        set
        {
            if (value == _filePath)
                return;
            _filePath = value;
            NewFilePath = true;
        }
    }
    string _filePath = "";

    private bool NewFilePath { get; set; } = true;

    internal void UnknownError(Element element)
    {
        Append($"unknown error at line({element.Line}), column({element.Column})");
    }

    internal void UnexpectedName(Element element)
    {
        Append($"unexpected name at line({element.Line}), column({element.Column})");
    }

    internal void UnexpectedOperator(Element element)
    {
        Append($"unexpected operator at line({element.Line}), column({element.Column})");
    }

    internal void UnexpectedValue(Element element)
    {
        Append($"unexpected value at line({element.Line}), column({element.Column})");
    }

    internal void UnexpectedArrayType(Element element)
    {
        Append($"unexpected array type at line({element.Line}), column({element.Column})");
    }

    internal void UnexpectedArraySyntax(Element element)
    {
        Append($"unexpected array syntax at line({element.Line}), column({element.Column})");
    }

    internal void Exception(string message)
    {
        Append(message);
    }

    private void Append(string message)
    {
        if (NewFilePath)
        {
            Log.AppendLine(FilePath);
            NewFilePath = false;
        }
        Log.AppendLine($"\t{message}");
    }
}