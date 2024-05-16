using LocalUtilities.SimpleScript.Data;
using System.Text;

namespace LocalUtilities.SimpleScript.Parser;

internal class Tokenizer
{
    const char Note = '#';
    const char Quote = '"';
    const char Escape = '\\';
    static char[] Delimiter { get; } = ['\t', ' ', '\n', '\r', /*',',*/ '#', '=', '>', '<', '}', '{', '"', '\0'];
    static char[] Blank { get; } = ['\t', ' ', '\n', '\r', /*',',*/ '\0'];
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

    States State { get; set; } = States.None;

    byte[] Buffer { get; set; } = [];

    int BufferPosition { get; set; } = 0;

    int Line { get; set; } = 1;

    int Column { get; set; } = 0;

    ParseTree Tree { get; set; } = new();

    Token Composed { get; set; } = new();

    StringBuilder Composing { get; } = new();

    internal ElementScope Elements { get; } = new(/*null, */new(), new(), new(), -1);

    internal Tokenizer(byte[] bytes)
    {
        Buffer = bytes;
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
                Tree = new();
            }
            else
                Tree = tree;
        }
        if (Tree.From is not null)
            throw new SsParseExceptions($"interruption at line({Line}), column({Column})");
        AddToken();
        void AddToken()
        {
            var token = Tree.Submit();
            if (token is not null)
                Elements.Append(token);
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
                        Composing.Append(GetU8Char());
                        State = States.Escape;
                        return false;
                    case Quote:
                        //Composing.Append(GetChar());
                        Composed = new(Composing.ToString(), Line, Column);
                        State = States.None;
                        GetU8Char();
                        return true;
                    case { } when EndLine.Contains(ch):
                        Composing.Append(Quote);
                        Composed = new(Composing.ToString(), Line, Column);
                        State = States.None;
                        return true;
                }
                Composing.Append(GetU8Char());
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
                    Composing.Append(GetU8Char());
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
                Composing.Append(GetU8Char());
                return false;
            case States.Note:
                if (EndLine.Contains(ch))
                {
                    State = States.None;
                }
                GetU8Char();
                return false;
            default:
                switch (ch)
                {
                    case Quote:
                        Composing.Clear();
                        //Composing.Append(GetChar());
                        GetU8Char();
                        State = States.Quotation;
                        break;
                    case Note:
                        State = States.Note;
                        GetU8Char();
                        break;
                    case { } when Marker.Contains(ch):
                        Composed = new(GetU8Char().ToString(), Line, Column);
                        return true;
                    case { } when Blank.Contains(ch):
                        GetU8Char();
                        break;
                    default:
                        Composing.Clear();
                        Composing.Append(GetU8Char());
                        State = States.Word;
                        break;
                }
                return false;
        }
    }

    /// <summary>
    /// 获取UTF-8字符长度
    /// <para>U-00000000 - U-0000007F: 0xxxxxxx</para>
    /// <para>U-00000080 - U-000007FF: 110xxxxx 10xxxxxx</para>
    /// <para>U-00000800 - U-0000FFFF: 1110xxxx 10xxxxxx 10xxxxxx</para>
    /// <para>U-00010000 - U-001FFFFF: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx</para>
    /// <para>U-00200000 - U-03FFFFFF: 111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx</para>
    /// <para>U-04000000 - U-7FFFFFFF: 1111110x 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx</para>
    /// </summary>
    /// <param name="head">起始位字符</param>
    /// <returns>返回1-6</returns>
    /// <exception cref="无效的UTF-8起始符"></exception>
    private string GetU8Char()
    {
        var length = 0;
        byte mask = 0b10000000;
        while ((Buffer[BufferPosition] & mask) is not 0)
        {
            if (++length > 6)
                throw new ArgumentException("无效的UTF-8起始符。");
            mask >>= 1;
        }
        //
        // ASCII's 8th bit is 0, and head of two-bytes utf-8's is 110xxxxx, so variable 'length' itself will be 0 or 2...6
        //
        if (length is 0)
            length = 1;
        var str = new List<byte>();
        for (int i = 0; i < length; i++)
            str.Add(Buffer[BufferPosition++]);
        if (str[0] is (byte)'\n')
        {
            Line++;
            Column = 0;
        }
        else if (str[0] is (byte)'\t')
            Column += 4;
        else
            Column += length;
        return Encoding.UTF8.GetString(str.ToArray());
    }
}