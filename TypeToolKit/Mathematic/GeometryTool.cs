using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeToolKit.Mathematic;

public static class GeometryTool
{
    public static Size ScaleSizeOnRatio(this Size fromSize, Size toSize)
    {
        var toWidth = toSize.Width;
        var toHeight = toSize.Height;
        var toRatio = toSize.Width / (double)toSize.Height;
        var sourceRatio = fromSize.Width / (double)fromSize.Height;
        if (sourceRatio > toRatio)
        {
            toWidth = toSize.Width;
            toHeight = (int)(toWidth / sourceRatio);
        }
        else if (sourceRatio < toRatio)
        {
            toHeight = toSize.Height;
            toWidth = (int)(toHeight * sourceRatio);
        }
        return new(toWidth, toHeight);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="range"></param>
    /// <param name="result">not null when return true, and the value is <paramref name="target"/> cutten within <paramref name="range"/>; otherwise null</param>
    /// <returns>if <paramref name="target"/> is out of <paramref name="target"/> will return false, otherwise will return true</returns>
    public static bool CutRectInRange(this Rectangle target, Rectangle range, [NotNullWhen(true)] out Rectangle? result)
    {
        result = null;
        var left = target.Left;
        var right = target.Right;
        var top = target.Top;
        var bottom = target.Bottom;
        if (left < range.Left)
        {
            if (right <= range.Left)
                return false;
            left = range.Left;
        }
        if (right > range.Right)
        {
            if (left >= range.Right)
                return false;
            right = range.Right;
        }
        if (top < range.Top)
        {
            if (bottom <= range.Top)
                return false;
            top = range.Top;
        }
        if (bottom > range.Bottom)
        {
            if (top >= range.Bottom)
                return false;
            bottom = range.Bottom;
        }
        result = new(left, top, right - left, bottom - top);
        return true;
    }
}
