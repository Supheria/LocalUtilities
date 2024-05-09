using System.Text;

namespace LocalUtilities.StringUtilities;

public static class StringBuilderTool
{
    static char[] Blanks { get; } = ['\\', '"', '#', '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', ',', '\0'];

    private static string GetQuote(this string str)
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

    public static StringBuilder AppendTab(this StringBuilder sb, int times)
    {
        for (var i = 0; i < times; i++)
            sb.Append('\t');
        return sb;
    }

    public static StringBuilder AppendNameStart(this StringBuilder sb, int level, string name)
    {
        return sb.AppendTab(level)
            .Append($"{name.GetQuote()}={{\n");
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level)
    {
        return sb.AppendTab(level)
            .Append("}\n");
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<string> values)
    {
        return sb.AppendTab(level)
            .Append($"{name.GetQuote()}={tag.GetQuote()}{(values.Count is 0 ? "" : '{')}")
            .AppendJoin(' ', values, (sb, value) =>
            {
                sb.Append(value.GetQuote());
            })
            .Append($"{(values.Count is 0 ? "" : '}')}\n");
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<string>> valuesArray)
    {
        return sb.AppendTab(level)
            .Append($"{name.GetQuote()}={{\n")
            .AppendJoin('\0', valuesArray, (sb, values) =>
            {
                sb.AppendTab(level + 1)
                .Append('{')
                .AppendJoin(' ', values, (sb, value) =>
                {
                    sb.Append(value.GetQuote());
                })
                .Append("}\n");
            })
            .AppendTab(level)
            .Append("}\n");
    }

    public static StringBuilder AppendTagValuesPairsArray(this StringBuilder sb, int level, string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        return sb.AppendTab(level)
            .Append($"{name.GetQuote()}={{\n")
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
                        sb.Append(value.GetQuote());
                    })
                    .Append('}');
                })
                .Append("}\n");
            })
            .AppendTab(level);
    }
}