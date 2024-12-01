namespace LocalUtilities.General;

public static class SimpleTypes
{
    public static byte ToByte(this string str)
    {
        _ = byte.TryParse(str, out var value);
        return value;
    }

    public static char ToChar(this string str)
    {
        _ = char.TryParse(str, out var value);
        return value;
    }

    public static short ToShort(this string str)
    {
        _ = short.TryParse(str, out var value);
        return value;
    }

    public static int ToInt(this string str)
    {
        _ = int.TryParse(str, out var value);
        return value;
    }

    public static long ToLong(this string str)
    {
        _ = long.TryParse(str, out var value);
        return value;
    }

    public static float ToFloat(this string str)
    {
        _ = float.TryParse(str, out var value);
        return value;
    }

    public static double ToDouble(this string str)
    {
        _ = double.TryParse(str, out var value);
        return value;
    }

    public static bool ToBool(this string str)
    {
        _ = bool.TryParse(str, out var value);
        return value;
    }
}
