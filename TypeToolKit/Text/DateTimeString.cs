namespace LocalUtilities.TypeToolKit.Text;

public static class DateTimeString
{
    public static string GetFormatString(this DateTime dateTime)
    {
        return $"{dateTime:yyyyMMddHHmmss}";
    }
}
