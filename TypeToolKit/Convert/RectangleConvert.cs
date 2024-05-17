namespace LocalUtilities.TypeToolKit.Convert;

public static class RectangleConvert
{
    public static string ToArrayString(this Rectangle rect)
    {
        return StringArray.ToArrayString(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static Rectangle ToRectangle(this string str)
    {
        var list = str.ToArray();
        if (list.Length is 4)
            return new(int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[2]), int.Parse(list[3]));
        throw TypeConvertException.CannotConvertStringTo<Rectangle>();
    }
}
