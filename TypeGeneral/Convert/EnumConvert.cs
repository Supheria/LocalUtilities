using System.ComponentModel;
using System.Reflection;

namespace LocalUtilities.TypeGeneral.Convert;

public static class EnumConvert
{
    public static T ToEnum<T>(this string str) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), str);
    }

    public static T DescriptionToEnum<T>(this string? str, T @default) where T : Enum
    {
        if (str is null)
            return @default;
        var map = new Dictionary<string, string>();
        var fieldinfos = typeof(T).GetFields();
        foreach (FieldInfo field in fieldinfos)
        {
            var atts = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (atts is null || atts.Length is 0)
                continue;
            map[((DescriptionAttribute)atts[0]).Description] = field.Name;
        }
        if (!map.TryGetValue(str, out var e))
            return @default;
        return e.ToEnum<T>();
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
