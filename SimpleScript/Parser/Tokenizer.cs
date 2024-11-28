using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Parser;

internal class Tokenizer
{
    private enum States
    {
        None,
        Quotation,
        Escape,
        Word,
        Note
    }

    States State { get; set; } = States.None;

    string Buffer { get; set; }

    int BufferPosition { get; set; } = 0;

    int Line { get; set; } = 1;

    int Column { get; set; } = 0;

    ParseTree Tree { get; set; }

    Token Composed { get; set; } = new();

    StringBuilder Composing { get; } = new();

    public Element Element { get; } = new(/*null, */new(), new(), new(), -1);

    SignTable SignTable { get; }

    public Tokenizer(byte[] buffer, int offset, int count, SignTable signTable, Encoding encoding)
    {
        Buffer = encoding.GetString(buffer, offset, count);
        SignTable = signTable;
        Tree = new(SignTable);
        Tokenize();
    }

    public Tokenizer(string str, SignTable signTable)
    {
        Buffer = str;
        SignTable = signTable;
        Tree = new(SignTable);
        Tokenize();
    }

    private void Tokenize()
    {
        while (BufferPosition < Buffer.Length)
        {
            if (!Compose((char)Buffer[BufferPosition]))
                continue;
            var tree = Tree.Parse(Composed);
            if (tree is null)
            {
                AddToken();
                Tree = new(SignTable);
            }
            else
                Tree = tree;
        }
        if (Tree.From is not null)
            throw new SsParseException($"interruption at line({Line}), column({Column})");
        if (!Tree.IsDone)
            Tree.Parse(new());
        AddToken();
        void AddToken()
        {
            var element = Tree.Submit();
            if (element is not null)
                Element.Append(element);
        }
    }

    private bool Compose(char ch)
    {
        if (!Composed.Submitted)
            return true;
        switch (State)
        {
            case States.Quotation:
                if (ch == SignTable.Escape)
                {
                    State = States.Escape;
                    GetChar();
                    return false;
                }
                if (ch == SignTable.Quote)
                {
                    Composed = new(Composing.ToString(), Line, Column, true);
                    State = States.None;
                    GetChar();
                    return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Escape:
                if (ch == SignTable.Return ||
                    ch == SignTable.NewLine ||
                    ch == SignTable.Empty)
                {
                    Composed = new(Composing.ToString(), Line, Column, true);
                    State = States.None;
                    return true;
                }
                Composing.Append(GetChar());
                State = States.Quotation;
                return false;
            case States.Word:
                if (ch == SignTable.Tab ||
                    ch == SignTable.Space ||
                    ch == SignTable.Return ||
                    ch == SignTable.NewLine ||
                    ch == SignTable.Note ||
                    ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less ||
                    ch == SignTable.Open ||
                    ch == SignTable.Close ||
                    ch == SignTable.Quote ||
                    ch == SignTable.Empty)
                {
                    Composed = new(Composing.ToString(), Line, Column, false);
                    State = States.None;
                    return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Note:
                if (ch == SignTable.Return ||
                    ch == SignTable.NewLine ||
                    ch == SignTable.Empty)
                {
                    State = States.None;
                    GetChar();
                    return false;
                }
                GetChar();
                return false;
            default:
                if (ch == SignTable.Quote)
                {
                    Composing.Clear();
                    GetChar();
                    State = States.Quotation;
                    return false;
                }
                if (ch == SignTable.Note)
                {
                    State = States.Note;
                    GetChar();
                    return false;
                }
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less ||
                    ch == SignTable.Open ||
                    ch == SignTable.Close)
                {
                    Composed = new(GetChar().ToString(), Line, Column, false);
                    return true;
                }
                if (ch == SignTable.Tab ||
                    ch == SignTable.Space ||
                    ch == SignTable.Return ||
                    ch == SignTable.NewLine ||
                    ch == SignTable.Empty)
                {
                    GetChar();
                    return false;
                }
                Composing.Clear();
                Composing.Append(GetChar());
                State = States.Word;
                return false;
        }
    }

    private char GetChar()
    {
        var ch = Buffer[BufferPosition++];
        if (ch is '\n')
        {
            Line++;
            Column = 0;
        }
        else if (ch is '\t')
            Column += 4;
        else
            Column++;
        return ch;
    }
}