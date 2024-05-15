namespace LocalUtilities.SimpleScript.Parser;

internal class Token
{
    private string Text { get; }

    public bool Submitted { get; private set; }

    public int Line { get; }

    public int Column { get; }

    public Token()
    {
        Text = "";
        Submitted = true;
    }

    public Token(string text, int line, int column)
    {
        Text = text;
        Line = line;
        Column = column;
    }

    public char Head()
    {
        return Text.FirstOrDefault();
    }

    public Word Get()
    {
        Submitted = true;
        return new(Text, Line, Column);
    }

    public override string ToString()
    {
        return $"\"{Text}\" at Line({Line}), Column({Column})";
    }
}