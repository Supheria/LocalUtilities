using System.ComponentModel;
using System.Reflection;

namespace LocalUtilities.General;

public static class EnumConvert
{
    public static object? ToEnum(this string str, Type type)
    {
        try
        {
            if (Enum.TryParse(type, str, true, out var result))
                return result;
        }
        catch { }
        return null;
    }

    public static object? DescriptionToEnum(this string? str, Type type)
    {
        if (str is null)
            return null;
        var map = new Dictionary<string, string>();
        var fieldinfos = type.GetFields();
        foreach (FieldInfo field in fieldinfos)
        {
            var atts = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (atts is null || atts.Length is 0)
                continue;
            map[((DescriptionAttribute)atts[0]).Description] = field.Name;
        }
        if (!map.TryGetValue(str, out var e))
            return null;
        return e.ToEnum(type);
    }

    public static string GetDescription<T>(this T @enum) where T : Enum
    {
        var name = @enum.ToString();
        var fieldinfos = typeof(T).GetFields();
        foreach (FieldInfo field in fieldinfos)
        {
            if (field.Name != name)
                continue;
            var atts = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (atts is null || atts.Length is 0)
                return "";
            return ((DescriptionAttribute)atts[0]).Description;
        }
        return "";
    }

    public static string ToWholeString(this Enum @enum)
    {
        return $"{@enum.GetType().Name}.{@enum}";
    }
}
