using LocalUtilities.SimpleScript.Parser;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LocalUtilities.TypeBundle;

public static class StringBuilderTool
{
    const char Comment = '#';

    static char[] Blanks { get; } = ['\\', '"', '#', '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', /*',',*/ '\0'];

    public static string ToQuoted(this string str)
    {
        if (str is "")
            return toQuoted();
        foreach (var blank in Blanks)
        {
            if (str.Contains(blank))
                return toQuoted();
        }
        return str;
        string toQuoted()
        {
            return new StringBuilder()
                .Append('"')
                .Append(str)
                .Append('"')
                .ToString();
        };
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder sb, string separator, IList<T> source, Action<StringBuilder, T> func)
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
            return sb.Append("");
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

    public static StringBuilder AppendNameStart(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted()}={{")
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendArrayStart(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append('{')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendArrayEnd(this StringBuilder sb, int level, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTag(this StringBuilder sb, int level, string name, string tag, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted()}={tag.ToQuoted()}")
            .Append($"{(tag is "" ? "" : " ")}")
            .AppendNewLine(writeIntoMultiLines); ;
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<string> values, bool writeIntoMultiLines)
    {
        sb.AppendTab(level, writeIntoMultiLines);
        if (name is not "")
            sb.Append($"{name.ToQuoted()}=");
        return sb.Append($"{tag.ToQuoted()}{{")
            .AppendJoin(" ", values, (sb, value) =>
            {
                sb.AppendNewLine(writeIntoMultiLines)
                .AppendTab(level + 1, writeIntoMultiLines)
                .Append(value.ToQuoted());
            })
            .AppendNewLine(writeIntoMultiLines)
            .AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValues(this StringBuilder sb, int level, string name, List<string> values, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted()}={{")
            .AppendJoin("", values, (sb, value) =>
            {
                sb.AppendNewLine(writeIntoMultiLines)
                .AppendTab(level + 1, writeIntoMultiLines)
                .Append(value.ToQuoted());
            })
            .AppendNewLine(writeIntoMultiLines)
            .AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<string>> valueArrays, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted()}={{")
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin("", valueArrays, (sb, values) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append('{')
                .AppendJoin(" ", values, (sb, value) =>
                {
                    sb.Append(value.ToQuoted());
                })
                .Append('}')
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }
}