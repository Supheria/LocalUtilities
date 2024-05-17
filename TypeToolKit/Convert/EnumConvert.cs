using System.ComponentModel;
using System.Reflection;

namespace LocalUtilities.TypeToolKit.Convert;

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
        var map = GetEnumDescriptionList<T>();
        if (!map.TryGetValue(str, out var e))
            return @default;
        return e.ToEnum<T>();
    }

    /// <summary>
    /// 返回 <描述, 枚举项> 词典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static Dictionary<string, string> GetEnumDescriptionList<T>() where T : Enum
    {
        var map = new Dictionary<string, string>();
        var fieldinfos = typeof(T).GetFields();
        foreach (FieldInfo field in fieldinfos)
        {
            object[] atts = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (atts == null || atts.Length == 0) // 无描述
                continue;
            map.Add(((DescriptionAttribute)atts[0]).Description, field.Name);
        }
        return map;
    }
}
