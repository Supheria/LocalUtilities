using LocalUtilities.TypeToolKit.Convert;
using System.Collections;
using System.Text;

namespace LocalUtilities.TypeToolKit.Convert;

public static class ArrayString
{
    const char Splitter = ',';

    public static string[] ToArray(this string? str)
    {
        return str is null
            ? []
            : str.Split(Splitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    }

    public static string ToArrayString<T1, T2>(T1 item1, T2 item2)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString());
    }

    public static string ToArrayString<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString());
    }

    public static string ToArrayString<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
    {
        return ToArrayString(item1?.ToString(), item2?.ToString(), item3?.ToString(), item4?.ToString());
    }

    public static string ToArrayString(params string?[] array)
    {
        return array.ToArrayString();
    }

    public static string ToArrayString(this ICollection array)
    {
        if (array.Count is 0)
            return "";
        var sb = new StringBuilder();
        var enumer = array.GetEnumerator();
        enumer.MoveNext();
        sb.Append(enumer.Current);
        for (var i = 1; i < array.Count; ++i)
        {
            enumer.MoveNext();
            sb.Append(Splitter)
                .Append(enumer.Current);
        }
        return sb.ToString();
    }

    public static string ToArrayString<T1, T2>(this (T1 item1, T2 item2) pair)
    {
        return ToArrayString(pair.item1?.ToString(), pair.item2?.ToString());
    }
}
