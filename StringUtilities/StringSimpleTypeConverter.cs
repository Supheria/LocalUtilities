using LocalUtilities.RegexUtilities;
using System.ComponentModel;
using System.Text;

namespace LocalUtilities.StringUtilities;

public static class StringSimpleTypeConverter
{
    public const char ElementSplitter = ',';

    public static string ToArrayString<T>(this (T item1, T item2) pair) =>
        ToArrayString(pair.item1?.ToString(), pair.item2?.ToString());

    public static string ToArrayString<T1, T2>(T1 item1, T2 item2) =>
        ToArrayString(item1?.ToString(), item2?.ToString());

    public static string ToArrayString<T1, T2, T3>(T1 item1, T2 item2, T3 item3) =>
        ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString());

    public static string ToArrayString<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) =>
        ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString(), item4?.ToString());

    public static string ToArrayString(params string?[] array) => array.ToArrayString();

    public static string ToArrayString<T>(this IEnumerable<T?> array) =>
        new StringBuilder().AppendJoin(ElementSplitter, array.Select(x => x?.ToString() ?? "")).ToString();

    public static (T1, T2)? ToPair<T1, T2>(this string str, Func<string, T1> toItem1, Func<string, T2> toItem2)
    {
        if (RegexMatchTool.GetMatchIgnoreAllBlacks(str, @$"(.*){ElementSplitter}(.*)", out var match))
        {
            var item1 = toItem1(match.Groups[1].Value);
            var item2 = toItem2(match.Groups[2].Value);
            return (item1, item2);
        }
        return null;
    }

    public static Size ToSize(this string str, Size @default)
    {
        var pair = str.ToPair((s) => s.ToInt(@default.Width), (s) => s.ToInt(@default.Height));
        return pair is null ? @default : new(pair.Value.Item1, pair.Value.Item2);
    }

    public static Point ToPoint(this string str, Point @default)
    {
        var pair = str.ToPair((s) => s.ToInt(@default.X), (s) => s.ToInt(@default.Y));
        return pair is null ? @default : new(pair.Value.Item1, pair.Value.Item2);
    }

    public static string[] ToArray(this string? str) => str is null
        ? Array.Empty<string>()
        : str.Split(ElementSplitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
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

    public static T? DescriptionToEnum<T>(this string? str) where T : Enum
    {
        if (str is null)
            return default;
        var map = EnumDescriptionTool.GetEnumDescriptionList<T>();
        if (!map.TryGetValue(str, out var e))
            return default;
        return e.ToEnum<T>();
    }

    public static string[] ToDescriptionList<T>(this T e) where T : Enum
    {
        var map = EnumDescriptionTool.GetEnumDescriptionList<T>();
        return map.Keys.ToArray();
    }

    public static string ToDescription<T>(this T e) where T : Enum
    {
        string value = e.ToString();
        var field = typeof(T).GetField(value);
        var objs = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (objs == null || objs.Length == 0) // 无描述返回名称
            return value;
        return ((DescriptionAttribute)objs[0]).Description;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns>int.Parse fail will return null</returns>
    public static int ToInt(this string? str, int @defaut)
    {
        try
        {
            return str is null ? @defaut : int.Parse(str);
        }
        catch
        {
            return @defaut;
        }
    }

    public static bool ToBool(this string? str, bool @defaut)
    {
        try
        {
            return str is null ? @defaut : bool.Parse(str);
        }
        catch
        {
            return @defaut;
        }
    }

    public static float ToFloat(this string? str, float @defaut)
    {
        try
        {
            return str is null ? @defaut : float.Parse(str);
        }
        catch
        {
            return @defaut;
        }
    }

    public static double ToDouble(this string? str, double @defaut)
    {
        try
        {
            return str is null ? @defaut : double.Parse(str);
        }
        catch
        {
            return @defaut;
        }
    }
}