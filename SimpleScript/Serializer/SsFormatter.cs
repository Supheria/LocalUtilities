using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public static class SsFormatter
{
    private static string ToQuoted(this string str)
    {
        if (str is "")
            return new StringBuilder()
                .Append(SignTable.Quote)
                .Append(SignTable.Quote)
                .ToString();
        var sb = new StringBuilder();
        bool useQuote = false;
        foreach (var ch in str)
        {
            switch (ch)
            {
                case SignTable.Escape:
                case SignTable.Quote:
                    sb.Append(SignTable.Escape)
                        .Append(ch);
                    useQuote = true;
                    continue;
                case SignTable.Tab:
                case SignTable.Space:
                case SignTable.Note:
                case SignTable.Equal:
                case SignTable.Greater:
                case SignTable.Less:
                case SignTable.OpenBrace:
                case SignTable.CloseBrace:
                case SignTable.Return:
                case SignTable.NewLine:
                case SignTable.Empty:
                    useQuote = true;
                    sb.Append(ch);
                    continue;
                default:
                    sb.Append(ch);
                    continue;
            }
        }
        if (useQuote)
            return new StringBuilder()
                .Append(SignTable.Quote)
                .Append(sb)
                .Append(SignTable.Quote)
                .ToString();
        return sb.ToString();
    }

    private static StringBuilder AppendNewLine(this StringBuilder sb, bool writeIntoMultiLines)
    {
        if (writeIntoMultiLines)
            return sb.AppendLine();
        else
            return sb;
    }

    private static StringBuilder AppendTab(this StringBuilder sb, int times, bool writeIntoMultiLines)
    {
        if (writeIntoMultiLines)
        {
            for (var i = 0; i < times; i++)
                sb.Append(SignTable.Space)
                    .Append(SignTable.Space)
                    .Append(SignTable.Space)
                    .Append(SignTable.Space);
        }
        return sb;
    }

    private static byte[] Encode(StringBuilder sb)
    {
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static void AppendComment(this Stream stream, int level, string comment, bool writeIntoMultiLines)
    {
        var sb = new StringBuilder();
        if (writeIntoMultiLines)
        {
            sb.AppendTab(level, writeIntoMultiLines)
                .Append(SignTable.Note)
                .Append(comment)
                .AppendNewLine(writeIntoMultiLines);
            stream.Write(Encode(sb));
        }
    }

    public static void WriteName(this Stream stream, int level, string name, bool writeIntoMultiLines)
    {
        var sb = new StringBuilder()
            .AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted())
            .Append(SignTable.Equal);
        stream.Write(Encode(sb));
    }

    public static void WriteStart(this Stream stream, int level, bool writeIntoMultiLines)
    {
        var sb = new StringBuilder().AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.OpenBrace)
            .AppendNewLine(writeIntoMultiLines);
        stream.Write(Encode(sb));
    }

    public static void WriteEnd(this Stream stream, int level, bool writeIntoMultiLines)
    {
        var sb = new StringBuilder().AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
        stream.Write(Encode(sb));
    }

    public static void WriteValue(this Stream stream, int level, string value, bool writeIntoMultiLines)
    {
        var sb = new StringBuilder().AppendTab(level, writeIntoMultiLines)
            .Append(value.ToQuoted());
        if (value is not "")
            sb.Append(SignTable.Space);
        sb.AppendNewLine(writeIntoMultiLines);
        stream.Write(Encode(sb));
    }
}
