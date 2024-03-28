using System.ComponentModel;
using System.Reflection;

namespace LocalUtilities.StringUtilities;

internal static class EnumDescriptionTool
{
    /// <summary>
    /// 返回 <描述, 枚举项> 词典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<string, string> GetEnumDescriptionList<T>() where T : Enum
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
