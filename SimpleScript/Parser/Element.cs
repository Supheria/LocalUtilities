namespace LocalUtilities.SimpleScript.Parser;

internal class Element
{
    private string Text { get; }

    public bool Submitted { get; private set; }

    public int Line { get; }

    public int Column { get; }

    public Element()
    {
        Text = "";
        Submitted = true;
    }

    public Element(string text, int line, int column)
    {
        Text = text;
        Line = line;
        Column = column;
    }

    public char Head()
    {
        return Text.FirstOrDefault();
    }

    public string Get()
    {
        Submitted = true;
        return Text;
    }
}