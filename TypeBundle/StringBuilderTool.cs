﻿using LocalUtilities.SimpleScript.Parser;
using System.Text;

namespace LocalUtilities.TypeBundle;

public static class StringBuilderTool
{
    const char Comment = '#';

    static char[] Blanks { get; } = ['\\', '"', '#', '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', /*',',*/ '\0'];

    public static string ToQuoted(this string str, bool writeIntoMultiLines)
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

    public static StringBuilder AppendToken(this StringBuilder sb, int level, string name, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append(name.ToQuoted(writeIntoMultiLines))
            .Append(' ')
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

    public static StringBuilder AppendTagValue(this StringBuilder sb, int level, string name, string tag, IList<string> values, bool writeIntoMultiLines)
    {
        if(values.Count is 0)
        {
            return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={tag.ToQuoted(writeIntoMultiLines)}")
            .Append(' ')
            .AppendNewLine(writeIntoMultiLines);
        }
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={tag.ToQuoted(writeIntoMultiLines)}{{")
            .AppendJoin(" ", values, (sb, value) =>
            {
                sb.Append(value.ToQuoted(writeIntoMultiLines));
            })
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendValueArrays(this StringBuilder sb, int level, string name, List<List<string>> valueArrays, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={{")
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin("", valueArrays, (sb, values) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append('{')
                .AppendJoin(" ", values, (sb, value) =>
                {
                    sb.Append(value.ToQuoted(writeIntoMultiLines));
                })
                .Append('}')
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }

    public static StringBuilder AppendTagValueArrays(this StringBuilder sb, int level, string name, List<List<KeyValuePair<Word, List<Word>>>> pairsArray, bool writeIntoMultiLines)
    {
        return sb.AppendTab(level, writeIntoMultiLines)
            .Append($"{name.ToQuoted(writeIntoMultiLines)}={{")
            .AppendNewLine(writeIntoMultiLines)
            .AppendJoin("", pairsArray, (sb, pairs) =>
            {
                sb.AppendTab(level + 1, writeIntoMultiLines)
                .Append('{')
                .AppendJoin(" ", pairs, (sb, pair) =>
                {
                    sb.Append(pair.Key.Text)
                    .Append("={")
                    .AppendJoin(" ", pair.Value, (sb, value) =>
                    {
                        sb.Append(value.Text.ToQuoted(writeIntoMultiLines));
                    })
                    .Append('}');
                })
                .Append('}')
                .AppendNewLine(writeIntoMultiLines);
            })
            .AppendTab(level, writeIntoMultiLines)
            .Append('}')
            .AppendNewLine(writeIntoMultiLines);
    }
}