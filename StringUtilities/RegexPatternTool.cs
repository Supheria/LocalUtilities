using System.Text;
using LocalUtilities.RegexUtilities;

namespace LocalUtilities.StringUtilities;

public class RegexPatternTool
{
    public static string ReplacePatternStringParts(string? input, string[] partPatterns, Func<string, string> replace)
    {
        if (input is null)
            return "";
        var ignores = new HashSet<string>();
        foreach (var part in partPatterns)
        {
            var ignore = part;
            // remove the condition limit part of "look behind", some like (?<=pattern)
            if (RegexMatchTool.GetMatch(ignore, @"\(\?.+\)(.+)", out var m))
                ignore = m.Groups[1].Value;
            // if escaped also add the escape mark
            ignore = RegexMatchTool.GetMatch(ignore, @"\\.+", out m) ? m.Groups[0].Value[..2] : part[..1];
            ignores.Add(ignore);
        }
        var pattern = $"([^{new StringBuilder().AppendJoin("", ignores)}]*)({new StringBuilder().AppendJoin('|', partPatterns)})(.*)";
        StringBuilder sb;
        if (!RegexMatchTool.GetMatch(input, pattern, out var match))
            sb = new(input);
        else
        {
            sb = new();
            string post;
            do
            {
                sb.Append(match.Groups[1].Value);
                sb.Append(replace(match.Groups[2].Value));
                post = match.Groups[3].Value;
            } while (RegexMatchTool.GetMatch(post, pattern, out match));
            sb.Append(post);
        }
        return sb.ToString();
    }

    private static string ReplaceExclusiveOrUnlimitedCollection(string s) =>
        s.Substring(s.Length - 1, 1) is "." ? @"[^\s]" : s.Insert(s.Length - 1, @"\s");

    private static readonly string[] ExclusiveOrUnlimitedCollection =
    {
        @"\[\^[^]]+\]",
        @"(?<!\\)\.",
    };

    /// <summary>
    /// make such as "[^a]" to "[^a\s]", ".*" to "[^\s]*"
    /// it will ignore "\." that won't be "\[^\s]"
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static string ExcludeBlankInExclusiveOrUnlimitedCollection(string pattern) =>
        ReplacePatternStringParts(pattern, ExclusiveOrUnlimitedCollection, ReplaceExclusiveOrUnlimitedCollection);
}