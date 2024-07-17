namespace LocalUtilities.TypeToolKit.Convert;

public static class SizeConvert
{
    public static string ToArrayString(this Size size)
    {
        return (size.Width, size.Height).ToArrayString();
    }

    public static Size ToSize(this string? str)
    {
        var array = str.ToArray();
        if (array.Length is not 2 ||
            !int.TryParse(array[0], out var width) ||
            !int.TryParse(array[1], out var height))
            return new();
        return new(width, height);
    }

    public static string ToArrayString(this SizeF size)
    {
        return (size.Width, size.Height).ToArrayString();
    }

    public static SizeF ToSizeF(this string? str)
    {
        var array = str.ToArray();
        if (array.Length is not 2 ||
            !float.TryParse(array[0], out var width) ||
            !float.TryParse(array[1], out var height))
            return new();
        return new(width, height);
    }
}
