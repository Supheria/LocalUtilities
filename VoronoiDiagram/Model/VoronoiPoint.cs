using System.Runtime.CompilerServices;

namespace LocalUtilities.VoronoiDiagram.Model;

/// <summary>
/// The vertices/nodes of the Voronoi cells, i.e. the points equidistant to three or more Voronoi sites.
/// These are the end points of a <see cref="VoronoiEdge"/>.
/// These are the <see cref="VoronoiCell.CellVertices"/>.
/// Also used for some other derived locations.
/// </summary>
public class VoronoiPoint(double x, double y, Direction borderLocation = Direction.None)
{
    public double X { get; } = x;

    public double Y { get; } = y;

    /// <summary>
    /// Specifies if this point is on the border of the bounds and where.
    /// </summary>
    /// <remarks>
    /// Using this would be preferrable to comparing against the X/Y values due to possible precision issues.
    /// </remarks>
    public Direction BorderLocation { get; internal set; } = borderLocation;

    public double AngleTo(VoronoiPoint other)
    {
        return Math.Atan2(other.Y - Y, other.X - X);
    }

    public static implicit operator (int X, int Y)(VoronoiPoint point)
    {
        return ((int)point.X, (int)point.Y);
    }

#if DEBUG
    public override string ToString()
    {
        return
            "("
            + (X == double.MinValue ? "-∞" : X == double.MaxValue ? "+∞" : X.ToString("F3"))
            + ","
            + (Y == double.MinValue ? "-∞" : Y == double.MaxValue ? "+∞" : Y.ToString("F3"))
            + ")"
            + BorderLocationToString(BorderLocation);
    }

    public string ToString(string format)
    {
        return
            "("
            + (X == double.MinValue ? "-∞" : X == double.MaxValue ? "+∞" : X.ToString(format))
            + ","
            + (Y == double.MinValue ? "-∞" : Y == double.MaxValue ? "+∞" : Y.ToString(format))
            + ")"
            + BorderLocationToString(BorderLocation);
    }

    private static string BorderLocationToString(Direction location)
    {
        switch (location)
        {
            case Direction.LeftTop:
                return "LT";
            case Direction.Left:
                return "L";
            case Direction.LeftBottom:
                return "LB";
            case Direction.Bottom:
                return "B";
            case Direction.BottomRight:
                return "BR";
            case Direction.Right:
                return "R";
            case Direction.TopRight:
                return "TR";
            case Direction.Top:
                return "T";
            default:
                return "";
        }
    }
#endif
}

/// <remarks>
/// Note that these are ordered clock-wise starting at bottom-left
/// </remarks>
public enum Direction
{
    None,
    Left,
    Top,
    Right,
    Bottom,
    LeftTop,
    TopRight,
    LeftBottom,
    BottomRight,
}

internal static class VPointExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ApproxEqual(this VoronoiPoint value1, VoronoiPoint value2)
    {
        return
            value1.X.ApproxEqual(value2.X) &&
            value1.Y.ApproxEqual(value2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ApproxEqual(this VoronoiPoint value1, double x, double y)
    {
        return
            value1.X.ApproxEqual(x) &&
            value1.Y.ApproxEqual(y);
    }
}
