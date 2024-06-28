namespace LocalUtilities.TypeGeneral.Convert;

public static class PointConverter
{
    public static string ToArrayString(this Point point)
    {
        return ArrayString.ToArrayString(point.X, point.Y);
    }

    public static Point ToPoint(this string str)
    {
        var array = str.ToArray();
        if (array.Length is not 2 ||
            !int.TryParse(array[0], out var x) ||
            !int.TryParse(array[1], out var y))
            return new();
        return new(x, y);
    }
}
