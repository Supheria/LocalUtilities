using LocalUtilities.TypeToolKit.Convert;
using System.Text;

namespace LocalUtilities.TypeToolKit.Convert;

public static class StringArray
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

    public static string ToArrayString<T1, T2>(this (T1 item1, T2 item2) pair)
    {
        return ToArrayString(pair.item1?.ToString(), pair.item2?.ToString());
    }
}