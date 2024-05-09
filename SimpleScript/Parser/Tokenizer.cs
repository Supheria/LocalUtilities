using LocalUtilities.SimpleScript.Data;
using System.Text;

namespace LocalUtilities.SimpleScript.Parser;

internal class Tokenizer
{
    const char Note = '#';
    const char Quote = '"';
    const char Escape = '\\';
    static char[] Delimiter { get; } = ['\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', ',', '\0'];
    static char[] Blank { get; } = ['\t', ' ', '\n', '\r', ',', '\0'];
    static char[] EndLine { get; } = ['\n', '\r', '\0'];
    static char[] Marker { get; } = ['=', '>', '<', '}', '{'];

    private enum States
    {
        None,
        Quotation,
        Escape,
        Word,
        Note
    }

    Exceptions Exceptions { get; }

    States State { get; set; } = States.None;

    byte[] Buffer { get; set; } = [];

    int BufferPosition { get; set; }

    int Line { get; set; } = 1;

    int Column { get; set; }

    ParseTree Tree { get; set; }

    Element Composed { get; set; } = new();

    StringBuilder Composing { get; } = new();

    internal List<Token> Tokens { get; } = new();

    internal Tokenizer(Exceptions exceptions, string filePath)
    {
        Exceptions = exceptions;
        Tree = new(Exceptions);
        Parse(filePath);
    }

    private void Parse(string filePath)
    {
        ReadBuffer(filePath);
        Tree = new(Exceptions);
        while (BufferPosition < Buffer?.Length)
        {
            if (!Compose((char)Buffer[BufferPosition]))
                continue;
            var tree = Tree.Parse(Composed);
            if (tree is null)
            {
                CacheList();
                Tree = new(Exceptions);
            }
            else { Tree = tree; }
        }
        EndCheck();
    }

    private void ReadBuffer(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Exceptions.Exception($"could not open file: {filePath}");
            return;
        }
        using var file = File.OpenRead(filePath);
        if (file.ReadByte() == 0xEF && file.ReadByte() == 0xBB && file.ReadByte() == 0xBF)
        {
            Buffer = new byte[file.Length - 3];
            _ = file.Read(Buffer, 0, Buffer.Length);
        }
        else
        {
            file.Seek(0, SeekOrigin.Begin);
            Buffer = new byte[file.Length];
            _ = file.Read(Buffer, 0, Buffer.Length);
        }
    }

    private bool Compose(char ch)
    {
        if (!Composed.Submitted)
            return true;
        switch (State)
        {
            case States.Quotation:
                switch (ch)
                {
                    case Escape:
                        Composing.Append(GetChar());
                        State = States.Escape;
                        return false;
                    case Quote:
                        //Composing.Append(GetChar());
                        Composed = new(Composing.ToString(), Line, Column);
                        State = States.None;
                        return true;
                    case { } when EndLine.Contains(ch):
                        Composing.Append(Quote);
                        Composed = new(Composing.ToString(), Line, Column);
                        State = States.None;
                        return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Escape:
                if (EndLine.Contains(ch))
                {
                    //Composing.Append(Quote).Append(Quote);
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                else
                {
                    Composing.Append(GetChar());
                    State = States.Quotation;
                    return false;
                }
            case States.Word:
                if (Delimiter.Contains(ch))
                {
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Note:
                if (EndLine.Contains(ch))
                {
                    State = States.None;
                }
                GetChar();
                return false;
            default:
                switch (ch)
                {
                    case Quote:
                        Composing.Clear();
                        //Composing.Append(GetChar());
                        GetChar();
                        State = States.Quotation;
                        break;
                    case Note:
                        State = States.Note;
                        GetChar();
                        break;
                    case { } when Marker.Contains(ch):
                        Composed = new(GetChar().ToString(), Line, Column);
                        return true;
                    case { } when Blank.Contains(ch):
                        if (ch is '\n')
                        {
                            Line++;
                            Column = 0;
                        }
                        GetChar();
                        break;
                    default:
                        Composing.Clear();
                        Composing.Append(GetChar());
                        State = States.Word;
                        break;

                }
                return false;
        }
    }
    private void CacheList()
    {
        var token = Tree.OnceGet();
        if (token is NullToken)
            return;
        Tokens.Add(token);
    }

    private char GetChar()
    {
        var ch = (char)Buffer[BufferPosition++];
        if (ch == '\t')
            Column += 4;
        else
            Column++;
        return ch;
    }

    private void EndCheck()
    {
        if (Tree.From is not null)
        {
            Exceptions.Exception($"interruption at line({Line}), column({Column})");
            Tree.Done();
            Tree = Tree.From;
            while (Tree.From is not null)
            {
                Tree.Done();
                Tree = Tree.From;
            }
        }
        CacheList();
    }
}