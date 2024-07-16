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

    /// <summary>
    /// when <paramref name="writeIntoMultiLines"/> is false, comment won't be written out
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="level"></param>
    /// <param name="comment"></param>
    /// <param name="writeIntoMultiLines"></param>
    /// <returns></returns>
    public static StringBuilder AppendComment(this StringBuilder sb, int level, string comment, bool writeIntoMultiLines)
    {
        if (writeIntoMultiLines)
            return sb.AppendTab(level, writeIntoMultiLines)
                .Append(SignTable.Note)
                .Append(comment)
                .AppendNewLine(writeIntoMultiLines);
        return sb;
    }

    public static StringBuilder AppendName(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
           .Append(name.ToQuoted())
           .Append(SignTable.Equal);
    }

    public static StringBuilder AppendStart(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.OpenBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValue(this StringBuilder sb, int level, string value, bool writeIntoMultiLines)
    {
        sb.AppendTab(level, writeIntoMultiLines)
            .Append(value.ToQuoted());
        if (value is not "")
            sb.Append(SignTable.Space);
        return sb.AppendNewLine(writeIntoMultiLines);
    }
}
