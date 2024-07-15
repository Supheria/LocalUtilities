using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LocalUtilities.SimpleScript.Common;

public static class SsFormatter
{
    public static string ToQuoted(this string str)
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

    public static StringBuilder AppendNewLine(this StringBuilder sb, bool writeIntoMultiLines)
    {
        if (writeIntoMultiLines)
            return sb.AppendLine();
        else
            return sb;
    }

    public static StringBuilder AppendTab(this StringBuilder sb, int times, bool writeIntoMultiLines)
    {
        if (writeIntoMultiLines)
        {
            for (var i = 0; i < times; i++)
                sb.Append(SignTable.Tab);
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

    public static StringBuilder AppendNameStart(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted())
            .Append(SignTable.Equal)
            .Append(SignTable.OpenBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendArrayEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTag(this StringBuilder sb, int level, string name, string tag, bool writeIntoMultiLines)
    {
        sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted())
            .Append(SignTable.Equal)
            .Append(tag.ToQuoted());
        if (tag is not "")
            sb.Append(SignTable.Space);
        return sb.AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<string> values, bool writeIntoMultiLines)
    {
        sb.AppendTab(level, writeIntoMultiLines);
        if (name is not "")
            sb.Append(name.ToQuoted())
                .Append(SignTable.Equal);
        return sb.Append(tag.ToQuoted())
            .Append(SignTable.OpenBrace)
            .AppendJoin(SignTable.Space, values, (sb, value) =>
            {
                sb.AppendNewLine(writeIntoMultiLines)
                .AppendTab(level + 1, writeIntoMultiLines)
                .Append(value.ToQuoted());
            })
            .AppendNewLine(writeIntoMultiLines)
            .AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValues(this StringBuilder sb, int level, string name, List<string> values, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted())
            .Append(SignTable.Equal)
            .Append(SignTable.OpenBrace)
            .AppendJoin(SignTable.Space, values, (sb, value) =>
            {
                sb.AppendNewLine(writeIntoMultiLines)
                .AppendTab(level + 1, writeIntoMultiLines)
                .Append(value.ToQuoted());
            })
            .AppendNewLine(writeIntoMultiLines)
            .AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<string>> valueArrays, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted())
            .Append(SignTable.Equal)
            .Append(SignTable.OpenBrace)
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin(SignTable.Empty, valueArrays, (sb, values) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append(SignTable.OpenBrace)
                .AppendJoin(SignTable.Space, values, (sb, value) =>
                {
                    sb.Append(value.ToQuoted());
                })
                .Append(SignTable.CloseBrace)
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .Append(SignTable.CloseBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendName(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
           .Append(name.ToQuoted())
           .Append(SignTable.Equal);
    }

    public static StringBuilder AppendStart(this StringBuilder sb, bool  writeIntoMultiLines)
    {
        return sb.Append(SignTable.OpenBrace)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendArrayStart(this StringBuilder sb, int level, string? tag, bool writeIntoMultiLines)
    {
        sb.AppendTab(level, writeIntoMultiLines);
        if (tag is not null)
            sb.Append(tag.ToQuoted());
        return sb.Append(SignTable.OpenBrace)
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
        sb.Append(value.ToQuoted());
        if (value is not "")
            sb.Append(SignTable.Space);
        return sb.AppendNewLine(writeIntoMultiLines);
    }
}
