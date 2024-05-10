using System.Text;

namespace LocalUtilities.StringUtilities;

public static class StringBuilderTool
{
    static char[] Blanks { get; } = ['\\', '"', '#', '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', ',', '\0'];

    public static bool WriteInMultiLines { get; set; } = false;

    private static string ToQuoted(this string str)
    {
        if (WriteInMultiLines)
        {
            foreach (var blank in Blanks)
            {
                if (str.Contains(blank))
                    return new StringBuilder()
                        .Append('"')
                        .Append(str)
                        .Append('"')
                        .ToString();
            }
            return str;
        }
        else
            return new StringBuilder()
                .Append('"')
                .Append(str)
                .Append('"')
                .ToString();
    }

    public static StringBuilder AppendJoin<T>(this StringBuilder sb, char separator, List<T> source, Action<StringBuilder, T> func)
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

    public static StringBuilder AppendNewLine(this StringBuilder sb)
    {
        if (WriteInMultiLines)
            return sb.AppendLine();
        else
            return sb;
    }

    public static StringBuilder AppendTab(this StringBuilder sb, int times)
    {
        if (WriteInMultiLines)
        {
            for (var i = 0; i < times; i++)
                sb.Append('\t');
        }
        return sb;
    }

    public static StringBuilder AppendNameStart(this StringBuilder sb, int level, string name)
    {
        return sb.AppendTab(level)
            .Append($"{name.ToQuoted()}={{")
            .AppendNewLine();
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level)
    {
        return sb.AppendTab(level)
            .Append('}')
            .AppendNewLine();
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<string> values)
    {
        return sb.AppendTab(level)
            .Append($"{name.ToQuoted()}={tag.ToQuoted()}{(values.Count is 0 ? "" : '{')}")
            .AppendJoin(' ', values, (sb, value) =>
            {
                sb.Append(value.ToQuoted());
            })
            .Append($"{(values.Count is 0 ? "" : '}')}")
            .AppendNewLine();
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<string>> valuesArray)
    {
        return sb.AppendTab(level)
            .Append($"{name.ToQuoted()}={{")
            .AppendNewLine()
            .AppendJoin('\0', valuesArray, (sb, values) =>
            {
                sb.AppendTab(level + 1)
                .Append('{')
                .AppendJoin(' ', values, (sb, value) =>
                {
                    sb.Append(value.ToQuoted());
                })
                .Append('}')
                .AppendNewLine();
            })
            .AppendTab(level)
            .AppendNewLine();
    }

    public static StringBuilder AppendTagValuesPairsArray(this StringBuilder sb, int level, string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        return sb.AppendTab(level)
            .Append($"{name.ToQuoted()}={{")
            .AppendNewLine()
            .AppendJoin('\0', pairsArray, (sb, pairs) =>
            {
                sb.AppendTab(level + 1)
                .Append('{')
                .AppendJoin(' ', pairs, (sb, pair) =>
                {
                    sb.Append(pair.Key)
                    .Append("={")
                    .AppendJoin(' ', pair.Value, (sb, value) =>
                    {
                        sb.Append(value.ToQuoted());
                    })
                    .Append('}');
                })
                .Append('}')
                .AppendNewLine();
            })
            .AppendTab(level)
            .AppendNewLine();
    }
}