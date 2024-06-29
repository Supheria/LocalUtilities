namespace LocalUtilities.TypeToolKit.Text;

public static class DateTimeString
{
    public static string ToFileName(this DateTime dateTime)
    {
        return $"{dateTime:yyyyMMddHHmmss}";
    }

    public static string ToUiniformLook(this DateTime dateTime)
    {
        return $"{dateTime:yyyy.MM.dd HH:mm:ss}";
    }
}
