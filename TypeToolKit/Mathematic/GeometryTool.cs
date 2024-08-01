using LocalUtilities.TypeGeneral;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeToolKit.Mathematic;

public static class GeometryTool
{
    public static Size ScaleSizeWithinRatio(this Size fromSize, Size toSize)
    {
        var toWidth = toSize.Width;
        var toHeight = toSize.Height;
        var toRatio = toSize.Width / (double)toSize.Height;
        var sourceRatio = fromSize.Width / (double)fromSize.Height;
        if (sourceRatio > toRatio)
        {
            toWidth = toSize.Width;
            toHeight = (toWidth / sourceRatio).ToRoundInt();
        }
        else if (sourceRatio < toRatio)
        {
            toHeight = toSize.Height;
            toWidth = (toHeight * sourceRatio).ToRoundInt();
        }
        return new(toWidth, toHeight);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="range"></param>
    /// <returns> the value of <paramref name="source"/> cutten within <paramref name="range"/></returns>
    public static Rectangle CutRectInRange(this Rectangle source, Rectangle range)
    {
        var left = source.Left;
        var right = source.Right;
        var top = source.Top;
        var bottom = source.Bottom;
        if (left < range.Left)
        {
            if (right <= range.Left)
                return new();
            left = range.Left;
        }
        if (right > range.Right)
        {
            if (left >= range.Right)
                return new();
            right = range.Right;
        }
        if (top < range.Top)
        {
            if (bottom <= range.Top)
                return new();
            top = range.Top;
        }
        if (bottom > range.Bottom)
        {
            if (top >= range.Bottom)
                return new();
            bottom = range.Bottom;
        }
        return new(left, top, right - left - 1, bottom - top - 1);
    }

    /// <summary>
    /// guarantee vertexes of <paramref name="target"/> permanently site on <paramref name="range"/>
    /// </summary>
    /// <param name="target"></param>
    /// <param name="range"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    private static void SiteRectInRange(Rectangle target, Rectangle range, out int left, out int right, out int top, out int bottom)
    {
        left = range.Left + ((target.Left - range.Left) % range.Width);
        left = left < range.Left ? left + range.Width : left;
        right = range.Right + ((target.Right - range.Right) % range.Width);
        right = right > range.Right ? right - range.Width : right;
        top = range.Top + ((target.Top - range.Top) % range.Height);
        top = top < range.Top ? top + range.Height : top;
        bottom = range.Bottom + ((target.Bottom - range.Bottom) % range.Height);
        bottom = bottom > range.Bottom ? bottom - range.Height : bottom;
    }

    public static List<Edge> CutRectLoopEdgesInRange(this Rectangle target, Rectangle range)
    {
        if (target.Width is 0 || target.Height is 0)
            return [];
        SiteRectInRange(target, range, out var left, out var right, out var top, out var bottom);
        var edges = new List<Edge>();
        if (left < right)
            edges.AddRange([
                new(new(left, top), new(right, top)),
                new(new(left, bottom), new(right, bottom)),
                ]);
        else
            edges.AddRange([
                new(new(range.Left, top), new(right, top)),
                new(new(range.Left, bottom), new(right, bottom)),
                new(new(left, top), new(range.Right, top)),
                new(new(left, bottom), new(range.Right, bottom)),
                ]);
        if (top < bottom)
            edges.AddRange([
                new(new(left, top), new(left, bottom)),
                new(new(right, top), new(right, bottom)),
                ]);
        else
            edges.AddRange([
                new(new(left, range.Top), new(left, bottom)),
                new(new(right, range.Top), new(right, bottom)),
                new(new(left, top), new(left, range.Bottom)),
                new(new(right, top), new(right, range.Bottom)),
                ]);
        return edges;
    }

    public static List<Rectangle> CutRectLoopRectsInRange(Rectangle source, Rectangle range)
    {

        if (source.Width is 0 || source.Height is 0)
            return [];
        SiteRectInRange(source, range, out var left, out var right, out var top, out var bottom);
        if (left > right && top > bottom)
            return [
                new(range.Left, range.Top, right - range.Left, bottom - range.Top),
                new(range.Left, top, right - range.Left, range.Bottom - top),
                new(left, range.Top, range.Right - left, bottom - range.Top),
                new(left, top, range.Right - left, range.Bottom - top),
                ];
        if (left < right && top > bottom)
            return [
                new(left, range.Top, right - left, bottom - range.Top),
                new(left, top, right - left, range.Bottom - top),
                ];
        if (top < bottom && left > right)
            return [
                new(range.Left, top, right - range.Left, bottom - top),
                new(left, top, range.Right - left, bottom - top),
                ];
        return [new(left, top, right - left, bottom - top)];
    }
}
