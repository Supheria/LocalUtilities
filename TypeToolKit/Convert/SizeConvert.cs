namespace LocalUtilities.TypeToolKit.Convert;

public static class SizeConvert
{
    public static string ToArrayString(this Size size)
    {
        return (size.Width, size.Height).ToArrayString();
    }

    public static Size ToSize(this string? str)
    {
        var list = str.ToArray();
        if (list.Length is 2)
            return new(int.Parse(list[0]), int.Parse(list[1]));
        throw TypeConvertException.CannotConvertStringTo<Size>();
    }
}
