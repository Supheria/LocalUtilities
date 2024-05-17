namespace LocalUtilities.TypeGeneral.Convert;

public static class PointConverter
{
    public static string ToArrayString(this Point point)
    {
        return ArrayString.ToArrayString(point.X, point.Y);
    }

    public static Point ToPoint(this string str)
    {
        var list = str.ToArray();
        if (list.Length is 2)
            return new(int.Parse(list[0]), int.Parse(list[1]));
        throw TypeConvertException.CannotConvertStringTo<Point>();
    }
}
