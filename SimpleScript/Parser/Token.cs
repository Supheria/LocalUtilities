namespace LocalUtilities.SimpleScript.Parser;

internal class Token
{
    private string Text { get; }

    public bool Submitted { get; private set; }

    public int Line { get; }

    public int Column { get; }

    bool QuotedOrEscaped { get; } = false;

    public Token()
    {
        Text = "";
        Submitted = true;
    }

    public Token(string text, int line, int column, bool quotedOrEscaped)
    {
        Text = text;
        Line = line;
        Column = column;
        QuotedOrEscaped = quotedOrEscaped;
    }

    public char Head()
    {
        if (Text.Length is not 1 || QuotedOrEscaped)
            return '\0';
        return Text[0];
    }

    public Word Submit()
    {
        Submitted = true;
        return new(Text, Line, Column);
    }

    public override string ToString()
    {
        return $"\"{Text}\" at Line({Line}), Column({Column})";
    }
}