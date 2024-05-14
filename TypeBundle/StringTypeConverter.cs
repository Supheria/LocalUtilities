using LocalUtilities.MathBundle;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.TypeBundle;

public static class StringTypeConverter
{
    const char Splitter = ',';

    public static string[] ToArray(this string? str)
    {
        return str is null
            ? []
            : str.Split(Splitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    }

    //public static List<T>? ToCollection<T>(this string? str, Func<string, T> toT)
    //{
    //    if (str is null)
    //        return null;
    //    var array = str.ToArray();
    //    var list = new List<T>();
    //    foreach (var s in array)
    //    {
    //        var value = toT(s);
    //        if (value is not null)
    //            list.Add(value);
    //    }
    //    if (list.Count > 0)
    //        return list;
    //    return null;
    //}

    public static (T1, T2)? ToPair<T1, T2>(this string? str, Func<string, T1> toT1, Func<string, T2> toT2)
    {
        if (str is null)
            return null;
        var array = str.ToArray();
        if (array.Length is not 2)
            return null;
        return (toT1(array[0]), toT2(array[1]));
    }

    public static string ToArrayString<T1, T2>(this (T1 item1, T2 item2) pair)
    {
        return ToArrayString(pair.item1?.ToString(), pair.item2?.ToString());
    }

    public static string ToArrayString<T1, T2>(T1 item1, T2 item2)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString());
    }

    public static string ToArrayString<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString(), item4?.ToString());
    }

    public static string ToArrayString<T>(params T[] array)
    {
        return array.ToArrayString();
    }

    public static string ToArrayString(params string?[] array)
    {
        return array.ToArrayString();
    }

    public static string ToArrayString<T>(this ICollection<T?> array)
    {
        return new StringBuilder()
            .AppendJoin(Splitter, array.Select(x => x?.ToString() ?? ""))
            .ToString();
    }

    //private static List<T> ToTypeList<T>(this string str, Func<string, T> toT)
    //{
    //    var result = new List<T>();
    //    while (RegexMatchTool.GetMatchIgnoreAllBlacks(str, @$"([^,]*)(?:{Splitter}(.*))+", out var match))
    //    {
    //        result.Add(toT(match.Groups[1].Value));
    //        str = match.Groups[2].Value;
    //    }
    //    result.Add(toT(str));
    //    return result;
    //}

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

    public static Size ToSize(this string? str, Size @default)
    {
        if (str is null)
            return @default;
        var list = str.ToArray();
        if (list.Length is not 2)
            return @default;
        return new(list[0].ToInt(@default.Width), list[1].ToInt(@default.Height));
    }

    public static Point ToPoint(this string? str, Point @default)
    {
        if (str is null)
            return @default;
        var list = str.ToArray();
        if (list.Length is not 2)
            return @default;
        return new(list[0].ToInt(@default.X), list[1].ToInt(@default.Y));
    }

    public static Rectangle ToRectangle(this string? str, Rectangle @default)
    {
        if (str is null)
            return @default;
        var list = str.ToArray();
        if (list.Length is not 4)
            return @default;
        return new(list[0].ToInt(@default.X), list[1].ToInt(@default.Y), list[2].ToInt(@default.Width), list[3].ToInt(@default.Height));
    }

    public static Coordinate ToCoordinate(this string? str, Coordinate @default)
    {
        if (str is null)
            return @default;
        var list = str.ToArray();
        if (list.Length is not 2)
            return @default;
        return new(list[0].ToDouble(@default.X), list[1].ToDouble(@default.Y));
    }

    public static LatticedPoint ToLatticedPoint(this string? str, LatticedPoint @default)
    {
        if (str is null)
            return @default;
        var list = str.ToArray();
        if (list.Length is not 2)
            return @default;
        return new(list[0].ToInt(@default.Col), list[1].ToInt(@default.Row));
    }


    //////
    //////
    //////


    public static T ToEnum<T>(this string? str, T @default) where T : Enum
    {
        if (str is null)
            return @default;
        try
        {
            return (T)Enum.Parse(typeof(T), str);
        }
        catch
        {
            return @default;
        }
    }

    public static T DescriptionToEnum<T>(this string? str, T @default) where T : Enum
    {
        if (str is null)
            return @default;
        var map = EnumTool.GetEnumDescriptionList<T>();
        if (!map.TryGetValue(str, out var e))
            return @default;
        return e.ToEnum(@default);
    }

    public static int ToInt(this string? str, int? @default)
    {
        if (str is null)
            return @default ?? throw new StringTypeConvertException(typeof(int));
        try
        {
            return int.Parse(str);
        }
        catch
        {
            return @default ?? throw new StringTypeConvertException(typeof(int));
        }
    }

    public static bool ToBool(this string? str, bool @default)
    {
        if (str is null)
            return @default;
        try
        {
            return bool.Parse(str);
        }
        catch
        {
            return @default;
        }
    }

    public static float ToFloat(this string? str, float @default)
    {
        if (str is null)
            return @default;
        try
        {
            return float.Parse(str);
        }
        catch
        {
            return @default;
        }
    }

    public static double ToDouble(this string? str, double @default)
    {
        if (str is null)
            return @default;
        try
        {
            return double.Parse(str);
        }
        catch
        {
            return @default;
        }
    }
}