using System.Globalization;

namespace LocalUtilities.TypeToolKit.Text;

public static class DateTimeFormat
{
    public static string Data { get; } = "yyyyMMddHHmmssff";

    public static string Outlook { get; } = "yyyy.MM.dd HH:mm:ss.ffffff";

    public static DateTime ToDateTime(this string str, string format)
    {
        _ = DateTime.TryParseExact(str, format, null, DateTimeStyles.None, out var dateTime);
        return dateTime;
    }
}
