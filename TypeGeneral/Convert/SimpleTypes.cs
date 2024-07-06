namespace LocalUtilities.TypeGeneral.Convert;

public static class SimpleTypes
{
    public static int ToInt(this string str)
    {
        _ = int.TryParse(str, out int value);
        return value;
    }

    public static long ToLong(this string str)
    {
        _ = long.TryParse(str, out long value);
        return value;
    }

    public static float ToFloat(this string str)
    {
        _ = float.TryParse(str, out float value);
        return value;
    }

    public static double ToDouble(this string str)
    {
        _ = double.TryParse(str, out double value);
        return value;
    }

    public static bool ToBool(this string str)
    {
        _ = bool.TryParse(str, out bool value);
        return value;
    }
}
