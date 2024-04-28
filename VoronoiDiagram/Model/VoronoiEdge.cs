namespace LocalUtilities.VoronoiDiagram.Model;

/// <summary>
/// The line segment making the Voronoi cells, i.e. the points equidistant to the two nearest Voronoi sites.
/// These are the lines in the <see cref="VoronoiCell.CellEdges"/>.
/// This has <see cref="VoronoiPoint"/> end points, i.e. <see cref="Start"/> and <see cref="End"/>.
/// This has the two <see cref="VoronoiCell"/>s they separate, i.e. <see cref="Right"/> and <see cref="Left"/>.
/// This connects in a <see cref="Neighbours"/> node graph to other <see cref="VoronoiEdge"/>s, i.e. shares end points with them.
/// </summary>
public class VoronoiEdge
{
    /// <summary>
    /// One of the two points making up this line segment, the other being <see cref="End"/>.
    /// </summary>
    public VoronoiPoint Start { get; internal set; }

    /// <summary>
    /// One of the two points making up this line segment, the other being <see cref="Start"/>.
    /// </summary>
    public VoronoiPoint? End { get; internal set; } = null;

    /// <summary>
    /// One of the two sites that this edge separates, the other being <see cref="Left"/>.
    /// Can be null if this is a border edge and there are no sites within the bounds.
    /// </summary>
    public VoronoiCell? Right { get; } = null;

    /// <summary>
    /// One of the two sites that this edge separates, the other being <see cref="Right"/>.
    /// Can be null if this is a border edge.
    ///  </summary>
    public VoronoiCell? Left { get; } = null;

    internal double SlopeRise { get; }

    internal double SlopeRun { get; }

    internal double? Slope { get; } = null;

    internal double? Intercept { get; } = null;

    internal VoronoiEdge? LastBeachLineNeighbor { get; set; } // I am not entirely sure this is the right name for this, but I just want to make it clear it's not something usable publicly

    internal VoronoiEdge(VoronoiPoint start, VoronoiCell left, VoronoiCell right)
    {
        Start = start;
        Left = left;
        Right = right;

        // Suspending this check because this never happens
        // //for bounding box edges
        // if (left == null || right == null)
        //     return;

        //from negative reciprocal of slope of line from left to right
        //ala m = (left.y -right.y / left.x - right.x)
        SlopeRise = left.Site.X - right.Site.X;
        SlopeRun = -(left.Site.Y - right.Site.Y);
        Intercept = null;

        if (SlopeRise.ApproxEqual(0) || SlopeRun.ApproxEqual(0)) return;
        Slope = SlopeRise / SlopeRun;
        Intercept = start.Y - Slope * start.X;
    }

    internal VoronoiEdge(VoronoiPoint start, VoronoiPoint end, VoronoiCell? right)
    {
        Start = start;
        End = end;
        Right = right;

        // Don't bother with slope stuff if we are given explicit coords
    }
    /// <summary>
    /// The mid-point between <see cref="Start"/> and <see cref="End"/> points.
    /// </summary>
    public VoronoiPoint GetMid()
    {
        return new((Start.X + End!.X) / 2, (Start.Y + End.Y) / 2);
    }

    /// <summary>
    /// The length of this line segment, i.e. the distance between <see cref="Start"/> and <see cref="End"/> points.
    /// </summary>
    public double GetLength()
    {
        if (End is null)
            return 0;
        var xd = End.X - Start.X;
        var yd = End.Y - Start.Y;
        return Math.Sqrt(xd * xd + yd * yd);
    }


#if DEBUG
    public override string ToString()
    {
        return (Start?.ToString() ?? "NONE") + "->" + (End?.ToString() ?? "NONE");
    }

    public string ToString(string format)
    {
        return (Start?.ToString(format) ?? "NONE") + "->" + (End?.ToString(format) ?? "NONE");
    }
#endif
}
