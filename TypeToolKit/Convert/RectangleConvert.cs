namespace LocalUtilities.TypeToolKit.Convert;

public static class RectangleConvert
{
    public static string ToArrayString(this Rectangle rect)
    {
        return ArrayString.ToArrayString(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static Rectangle ToRectangle(this string str)
    {
        var array = str.ToArray();
        if (array.Length is not 4 ||
            !int.TryParse(array[0], out var x) ||
            !int.TryParse(array[1], out var y) ||
            !int.TryParse(array[2], out var width) ||
            !int.TryParse(array[3], out var height))
            return new();
        return new(x, y, width, height);
    }
}
