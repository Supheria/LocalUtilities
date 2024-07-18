using LocalUtilities.SimpleScript.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public static class SsFormatter
{
    private static string ToQuoted(this string str, SignTable signTable)
    {
        if (str is "")
            return new StringBuilder()
                .Append(signTable.Quote)
                .Append(signTable.Quote)
                .ToString();
        var sb = new StringBuilder();
        bool useQuote = false;
        foreach (var ch in str)
        {
            if (ch == signTable.Escape ||
                ch == signTable.Quote)
            {
                sb.Append(signTable.Escape)
                        .Append(ch);
                useQuote = true;
            }
            else if (ch == signTable.Tab ||
                ch == signTable.Space ||
                ch == signTable.Note ||
                ch == signTable.Equal ||
                ch == signTable.Greater ||
                ch == signTable.Less ||
                ch == signTable.Open ||
                ch == signTable.Close ||
                ch == signTable.Return ||
                ch == signTable.NewLine ||
                ch == signTable.Empty)
            {
                useQuote = true;
                sb.Append(ch);
                continue;
            }
            else
                sb.Append(ch);
        }
        if (useQuote)
            return new StringBuilder()
                .Append(signTable.Quote)
                .Append(sb)
                .Append(signTable.Quote)
                .ToString();
        return sb.ToString();
    }

    private static StringBuilder AppendNewLine(this StringBuilder sb, bool writeIntoMultiLines, SignTable signTable)
    {
        if (writeIntoMultiLines)
            return sb.AppendLine();
        else
            return sb.Append(signTable.Space);
    }

    private static StringBuilder AppendTab(this StringBuilder sb, int times, bool writeIntoMultiLines, SignTable signTable)
    {
        if (writeIntoMultiLines)
        {
            for (var i = 0; i < times; i++)
                sb.Append(signTable.Space)
                    .Append(signTable.Space)
                    .Append(signTable.Space)
                    .Append(signTable.Space);
        }
        return sb;
    }

    public static string GetComment(int level, string comment, bool writeIntoMultiLines, SignTable signTable)
    {
        if (writeIntoMultiLines)
        {
            return new StringBuilder()
                .AppendTab(level, writeIntoMultiLines, signTable)
                .Append(signTable.Note)
                .Append(comment)
                .AppendNewLine(writeIntoMultiLines, signTable)
                .ToString();
        }
        return "";
    }

    public static string GetName(int level, string name, bool writeIntoMultiLines, SignTable signTable)
    {
        return new StringBuilder()
            .AppendTab(level, writeIntoMultiLines, signTable)
            .Append(name.ToQuoted(signTable))
            .Append(signTable.Equal)
            .ToString();
    }

    public static string GetStart(int level, bool writeIntoMultiLines, SignTable signTable)
    {
        return new StringBuilder()
             .AppendTab(level, writeIntoMultiLines, signTable)
             .Append(signTable.Open)
             .AppendNewLine(writeIntoMultiLines, signTable)
             .ToString();
    }

    public static string GetEnd(int level, bool writeIntoMultiLines, SignTable signTable)
    {
        return new StringBuilder()
            .AppendTab(level, writeIntoMultiLines, signTable)
            .Append(signTable.Close)
            .AppendNewLine(writeIntoMultiLines, signTable)
            .ToString();
    }

    public static string GetValue(int level, string value, bool writeIntoMultiLines, SignTable signTable)
    {
        return new StringBuilder()
            .AppendTab(level, writeIntoMultiLines, signTable)
            .Append(value.ToQuoted(signTable))
            .AppendNewLine(writeIntoMultiLines, signTable)
            .ToString();
    }

    public static string GetUnquotedValue(string value, bool writeIntoMultiLines)
    {
        return new StringBuilder(value)
            .ToString();
    }
}
