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
                        new((x1 - i).ToInt(), y),
                        new((x1 + i).ToInt(), y)
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
                        new(x, (y1 - i).ToInt()),
                        new(x, (y1 + i).ToInt())
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
            list.Add(new(x.ToInt(), y.ToInt()));
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
        return [new(x1.ToInt(), y1.ToInt()), new(x2.ToInt(), y2.ToInt())];
    }
}
