using System.Text;

namespace LocalUtilities.StringUtilities;

public static class StringBuilderTool
{
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
            .Append($"{name}={{\n");
    }

    public static StringBuilder AppendNameEnd(this StringBuilder sb, int level)
    {
        return sb.AppendTab(level)
            .Append("}\n");
    }

    public static StringBuilder AppendTagValues(this StringBuilder sb, int level, string name, string tag, List<string> values)
    {
        return sb.AppendTab(level)
            .Append($"{name}=\"{tag}\"{(values.Count is 0 ? "" : '{')}")
            .AppendJoin(' ', values)
            .Append($"{(values.Count is 0 ? "" : '}')}\n");
    }

    public static StringBuilder AppendValuesArray(this StringBuilder sb, int level, string name, List<List<string>> valuesArray)
    {
        return sb.AppendTab(level)
            .Append($"{name}={{\n")
            .AppendJoin('\0', valuesArray, (sb, values) =>
            {
                sb.AppendTab(level + 1)
                .Append('{')
                .AppendJoin(' ', values)
                .Append("}\n");
            })
            .AppendTab(level)
            .Append("}\n");
    }

    public static StringBuilder AppendTagValuesPairsArray(this StringBuilder sb, int level, string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        return sb.AppendTab(level)
            .Append($"{name}={{\n")
            .AppendJoin('\0', pairsArray, (sb, pairs) =>
            {
                sb.AppendTab(level + 1)
                .Append('{')
                .AppendJoin(' ', pairs, (sb, pair) =>
                {
                    sb.Append(pair.Key)
                    .Append("={")
                    .AppendJoin(' ', pair.Value)
                    .Append('}');
                })
                .Append("}\n");
            })
            .AppendTab(level);
    }
}