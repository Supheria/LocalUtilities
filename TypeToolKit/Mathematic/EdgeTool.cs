using LocalUtilities.TypeGeneral;

namespace LocalUtilities.TypeToolKit.Mathematic;

public static class EdgeTool
{
    static double EdgeInnerDensityUnit { get; } = 0.25;

    public static List<Coordinate> GetInnerPoints(this Edge edge, double width)
    {
        var list = new List<Coordinate>();
        var x1 = edge.Starter.X;
        var y1 = edge.Starter.Y;
        var x2 = edge.Ender.X;
        var y2 = edge.Ender.Y;
        var widthHalf = width / 2;
        if (x1 == x2)
        {
            if (y1 == y2)
                return list;
            var (yMin, yMax) = (Math.Min(y1, y2), Math.Max(y1, y2));
            for (int y = yMin + 1; y < yMax; y++)
            {
                list.Add(new(x1, y));
                for (var i = EdgeInnerDensityUnit; i.ApproxLessThanOrEqualTo(widthHalf); i += EdgeInnerDensityUnit)
                    list.AddRange([
                        new((x1 - i).ToRoundInt(), y),
                        new((x1 + i).ToRoundInt(), y)
                        ]);
            }
            return list;
        }
        if (y1 == y2)
        {
            var (xMin, xMax) = (Math.Min(x1, x2), Math.Max(x1, x2));
            for (int x = xMin + 1; x < xMax; x++)
            {
                list.Add(new(x, y1));
                for (var i = EdgeInnerDensityUnit; i.ApproxLessThanOrEqualTo(widthHalf); i += EdgeInnerDensityUnit)
                    list.AddRange([
                        new(x, (y1 - i).ToRoundInt()),
                        new(x, (y1 + i).ToRoundInt())
                        ]);
            }
            return list;
        }
        Coordinate left, right;
        left = x1 < x2 ? edge.Starter : edge.Ender;
        right = x1 > x2 ? edge.Starter : edge.Ender;
        var (dX, dY) = (right.X - (double)left.X, right.Y - (double)left.Y);
        var slope = dY / dX;
        var stepUint = dX / Math.Max(Math.Abs(dX), Math.Abs(dY));
        for (double x = left.X; x.ApproxLessThanOrEqualTo(right.X); x += stepUint)
        {
            var y = slope * (x - left.X) + left.Y;
            list.Add(new(x.ToRoundInt(), y.ToRoundInt()));
            for (var i = EdgeInnerDensityUnit; i.ApproxLessThanOrEqualTo(widthHalf); i += EdgeInnerDensityUnit)
                list.AddRange(AppendWidthPoints(x, y, slope, i));
        }
        return list;
    }

    /// <summary>
    /// <para> get two points beside on given point and have distance of <paramref name="widthHalf"/>, </para> 
    /// <para> which locate on the line that is vertical to given line has slope of <paramref name="slope"/> </para> 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="slope"></param>
    /// <param name="widthHalf"></param>
    /// <returns></returns>
    private static List<Coordinate> AppendWidthPoints(double x, double y, double slope, double widthHalf)
    {
        var c = widthHalf * slope / Math.Sqrt((slope * slope) + 1);
        var x1 = x - c;
        var x2 = x + c;
        var y1 = y - (x1 - x) / slope;
        var y2 = y - (x2 - x) / slope;
        return [new(x1.ToRoundInt(), y1.ToRoundInt()), new(x2.ToRoundInt(), y2.ToRoundInt())];
    }

    //public static Rectangle GetCrossLineRectInRange(this Edge edge, double lineWidth, Rectangle range)
    //{
    //    var p1 = edge.Starter;
    //    var p2 = edge.Ender;
    //    Rectangle lineRect;
    //    if (p1.Y == p2.Y)
    //    {
    //        if (p1.X == p2.X)
    //            return new();
    //        var y = p1.Y - lineWidth / 2;
    //        var xMin = Math.Min(p1.X, p2.X);
    //        lineRect = new(xMin, (int)y, Math.Abs(p1.X - p2.X), (int)lineWidth);
    //    }
    //    else if (p1.X == p2.X)
    //    {
    //        var x = p1.X - lineWidth / 2;
    //        var yMin = Math.Min(p1.Y, p2.Y);
    //        lineRect = new((int)x, yMin, (int)lineWidth, Math.Abs(p1.Y - p2.Y));
    //    }
    //    else
    //        throw new ArgumentException($"{p1}=>{p2} is not a cross line");
    //    if (lineRect.CutRectInRange(range, out var r))
    //        return r.Value;
    //    return new();
    //}

    public static List<Edge> GetRectEdges(this Rectangle rect)
    {
        return [
            new(new(rect.Left, rect.Top), new(rect.Right, rect.Top)),
            new(new(rect.Left, rect.Bottom), new(rect.Right, rect.Bottom)),
            new(new(rect.Left, rect.Top), new(rect.Left, rect.Bottom)),
            new(new(rect.Right, rect.Top), new(rect.Right, rect.Bottom)),
            ];
    }

    /// <summary>
    /// it will +2 to <paramref name="penWidht"/> 
    /// to ensure the edge thick is always bigger than the result of converting <see cref="float"/> to <see cref="int"/>
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="penWidht"></param>
    /// <returns></returns>
    public static Rectangle GetCrossLineRect(this Edge edge, float penWidht)
    {
        var p1 = edge.Starter;
        var p2 = edge.Ender;
        var thick = (int)penWidht + 2;
        var thickHalf = thick / 2;
        if (p1.Y == p2.Y)
        {
            if (p1.X == p2.X)
                return new();
            var y = p1.Y - thickHalf;
            var xMin = Math.Min(p1.X, p2.X) - thickHalf;
            var width = Math.Abs(p1.X - p2.X) + thick;
            return new(xMin, y, width, thick);
        }
        else
        {
            var x = p1.X - thickHalf;
            var yMin = Math.Min(p1.Y, p2.Y) - thickHalf;
            var height = Math.Abs(p1.Y - p2.Y) + thick;
            return new(x, yMin, thick, height);
        }
    }
}
