using System.Reflection;

namespace LocalUtilities.SimpleScript;

public partial class SerializeTool
{
    static BindingFlags Authority { get; } = BindingFlags.Public /*| BindingFlags.NonPublic*/ | BindingFlags.Instance;

    static byte[] Utf8_BOM { get; } = [0xEF, 0xBB, 0xBF];

    private static bool NotSsItem(PropertyInfo property)
    {
        return property.GetCustomAttribute<SsIgnore>() is not null || property.SetMethod is null;
    }

    private static string GetSsItemName(PropertyInfo property)
    {
        return property.GetCustomAttribute<SsItem>()?.Name ?? property.Name;
    }
}
