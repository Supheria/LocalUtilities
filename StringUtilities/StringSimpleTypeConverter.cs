using System.Text;
using LocalUtilities.RegexUtilities;

namespace LocalUtilities.StringUtilities;

public static class StringSimpleTypeConverter
{
    public const char ElementSplitter = ',';

    public static string ToArrayString<T1, T2>(T1 item1, T2 item2) =>
        ToArrayString(item1?.ToString(), item2?.ToString());

    public static string ToArrayString<T1, T2, T3>(T1 item1, T2 item2, T3 item3) =>
        ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString());

    public static string ToArrayString<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) =>
        ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString(), item4?.ToString());

    public static string ToArrayString(params string?[] array) => array.ToArrayString();

    public static string ToArrayString<T>(this IEnumerable<T?> array) =>
        new StringBuilder().AppendJoin(ElementSplitter, array.Select(x => x?.ToString() ?? "")).ToString();

    public static (T1, T2) ToPair<T1, T2>(this string? pair, T1 defaultValue1, T2 defaultValue2,
        Func<string, T1> toItem1, Func<string, T2> toItem2)
    {
        if (pair is null)
            return (defaultValue1, defaultValue2);
        return RegexMatchTool.GetMatchIgnoreAllBlacks(pair, @$"\((.*)\){ElementSplitter}\((.*)\)",
            out var match)
            ? (toItem1(match.Groups[1].Value), toItem2(match.Groups[2].Value))
            : (defaultValue1, defaultValue2);
    }

    public static string[] ToArray(this string? str) => str is null
        ? Array.Empty<string>()
        : str.Split(ElementSplitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

    public static T? ToEnum<T>(this string? str) where T : Enum
    {
        try
        {
            return str is null ? default : (T)Enum.Parse(typeof(T), str);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns>int.Parse fail will return null</returns>
    public static int? ToInt(this string? str)
    {
        try
        {
            return str is null ? null : int.Parse(str);
        }
        catch
        {
            return null;
        }
    }

    public static bool? ToBool(this string? str)
    {
        try
        {
            return str is null ? null : bool.Parse(str);
        }
        catch
        {
            return null;
        }
    }
}