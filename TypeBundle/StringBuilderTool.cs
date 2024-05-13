using LocalUtilities.SimpleScript.Parser;
using System.Text;

namespace LocalUtilities.TypeBundle;

public static class StringBuilderTool
{
    const char Comment = '#';

    static char[] Blanks { get; } = ['\\', '"', '#', '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', ',', '\0'];

    public static string ToQuoted(this string str, bool writeIntoMultiLines)
    {
        if (str is "")
            return toQuoted();
        if (writeIntoMultiLines)
        {
            foreach (var blank in Blanks)
            {
                if (str.Contains(blank))
                    return toQuoted();
            }
            return str;
        }
        else
            return toQuoted();
        string toQuoted()
        {
            return new StringBuilder()
                .Append('"')
                .Append(str)
                .Append('"')
                .ToString();
        };
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder sb, char separator, IList<T> source, Action<StringBuilder, T> func)
    {
        if (source.Count is 0)
            return sb;
        if (source[0] is not null)
            func(sb, source[0]);
        for (var i = 1; i < source.Count; ++i)
        {
            sb.Append(separator);
            if (source[i] is not null)
                func(sb, source[i]);
        }
        return sb;
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
                sb.Append('\t');
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
                .Append(Comment)
                .Append(comment)
                .AppendNewLine(writeIntoMultiLines);
        return sb;
    }

    public static StringBuilder AppendToken(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted(writeIntoMultiLines))
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendNameStart(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={{")
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<Word> values, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={tag.ToQuoted(writeIntoMultiLines)}{(values.Count is 0 ? "" : '{')}")
            .AppendJoin(' ', values, (sb, value) =>
            {
                sb.Append(value.Text.ToQuoted(writeIntoMultiLines));
            })
            .Append($"{(values.Count is 0 ? "" : '}')}")
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<Word>> valuesArray, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={{")
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin('\0', valuesArray, (sb, values) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append('{')
                .AppendJoin(' ', values, (sb, value) =>
                {
                    sb.Append(value.Text.ToQuoted(writeIntoMultiLines));
                })
                .Append('}')
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTagValuesPairsArray(this StringBuilder sb, int level, string name, List<List<KeyValuePair<Word, List<Word>>>> pairsArray, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={{")
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin('\0', pairsArray, (sb, pairs) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append('{')
                .AppendJoin(' ', pairs, (sb, pair) =>
                {
                    sb.Append(pair.Key.Text)
                    .Append("={")
                    .AppendJoin(' ', pair.Value, (sb, value) =>
                    {
                        sb.Append(value.Text.ToQuoted(writeIntoMultiLines));
                    })
                    .Append('}');
                })
                .Append('}')
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .AppendNewLine(writeIntoMultiLines);
    }
}