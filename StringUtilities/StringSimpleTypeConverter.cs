using LocalUtilities.RegexUtilities;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalUtilities.StringUtilities;

public static class StringSimpleTypeConverter
{
    const char Splitter = ',';

    public static string ToArrayString<T>(this (T item1, T item2) pair)
    {
        return ToArrayString(pair.item1?.ToString(), pair.item2?.ToString());
    }

    public static string ToArrayString<T>(T item1, T item2)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString());
    }

    public static string ToArrayString<T>(T item1, T item2, T item3, T item4)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString(), item4?.ToString());
    }

    public static string ToArrayString(params string?[] array)
    {
        return array.ToArrayString();
    }

    public static string ToArrayString<T>(this IEnumerable<T?> array)
    {
        return new StringBuilder()
            .AppendJoin(Splitter, array.Select(x => x?.ToString() ?? ""))
            .ToString();
    }

    public static List<T> ToTypeList<T>(this string str, Func<string, T> toT)
    {
        var result = new List<T>();
        while (RegexMatchTool.GetMatchIgnoreAllBlacks(str, @$"([^,]*)(?:{Splitter}(.*))+", out var match))
        {
            //foreach(var )
            result.Add(toT(match.Groups[1].Value));
            str = match.Groups[2].Value;
        }
        result.Add(toT(str));
        return result;
    }

    public static string ToArrayString(this Size size)
    {
        return ToArrayString(size.Width, size.Height);
    }

    public static string ToArrayString(this Point point)
    {
        return ToArrayString(point.X, point.Y);
    }

    public static string ToArrayString(this Rectangle rect)
    {
        return ToArrayString(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static Size ToSize(this string str, Size @default)
    {
        var list = str.ToTypeList(s => s.ToInt());
        if (list.Count is not 2)
            return @default;
        return new(list[0] ?? @default.Width, list[1] ?? @default.Height);
    }

    public static Point ToPoint(this string str, Point @default)
    {
        var list = str.ToTypeList(s => s.ToInt());
        if (list.Count is not 2)
            return @default;
        return new(list[0] ?? @default.X, list[1] ?? @default.Y);
    }

    public static Rectangle ToRectangle(this string str, Rectangle @default)
    {
        var list = str.ToTypeList(s=>s.ToInt());
        if (list.Count is not 4)
            return @default;
        return new(list[0] ?? @default.X, list[1] ?? @default.Y, list[2] ?? @default.Width, list[3] ?? @default.Height);
    }

    public static string[] ToArray(this string? str) => str is null
        ? Array.Empty<string>()
        : str.Split(Splitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
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