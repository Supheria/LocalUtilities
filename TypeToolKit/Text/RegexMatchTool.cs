using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

namespace LocalUtilities.TypeGeneral.Text;

public static partial class RegexMatchTool
{
    public static bool GetMatch(this string input, string pattern, [NotNullWhen(true)] out Match? match)
    {
        match = null;
        try
        {
            match = Match(input, pattern);
            return match.Success;
        }
        catch
        {
            return false;
        }
    }

    [GeneratedRegex(@"\s")]
    private static partial Regex Blank();

    public static bool GetMatchIgnoreAllBlacks(this string input, string pattern, [NotNullWhen(true)] out Match? match)
    {
        return GetMatch(Blank().Replace(input, ""), pattern, out match);
    }
}