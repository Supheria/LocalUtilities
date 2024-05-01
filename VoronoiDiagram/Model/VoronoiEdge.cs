namespace LocalUtilities.VoronoiDiagram.Model;

/// <summary>
/// The line segment making the Voronoi cells, i.e. the points equidistant to the two nearest Voronoi sites.
/// These are the lines in the <see cref="VoronoiCell.Edges"/>.
/// This has <see cref="VoronoiVertex"/> end points, i.e. <see cref="Starter"/> and <see cref="Ender"/>.
/// This has the two <see cref="VoronoiCell"/>s they separate, i.e. <see cref="Right"/> and <see cref="Left"/>.
/// This connects in a <see cref="Neighbours"/> node graph to other <see cref="VoronoiEdge"/>s, i.e. shares end points with them.
/// </summary>
public class VoronoiEdge
{
    /// <summary>
    /// One of the two points making up this line segment, the other being <see cref="Ender"/>.
    /// </summary>
    public VoronoiVertex Starter { get; internal set; }

    /// <summary>
    /// One of the two points making up this line segment, the other being <see cref="Starter"/>.
    /// </summary>
    public VoronoiVertex? Ender { get; internal set; } = null;

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

    internal VoronoiEdge(VoronoiVertex start, VoronoiCell left, VoronoiCell right)
    {
        Starter = start;
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

    internal VoronoiEdge(VoronoiVertex start, VoronoiVertex end, VoronoiCell? right)
    {
        Starter = start;
        Ender = end;
        Right = right;

        // Don't bother with slope stuff if we are given explicit coords
    }
    /// <summary>
    /// The mid-point between <see cref="Starter"/> and <see cref="Ender"/> points.
    /// </summary>
    public VoronoiVertex GetMid()
    {
        return new((Starter.X + Ender!.X) / 2, (Starter.Y + Ender.Y) / 2);
    }

    /// <summary>
    /// The length of this line segment, i.e. the distance between <see cref="Starter"/> and <see cref="Ender"/> points.
    /// </summary>
    public double GetLength()
    {
        if (Ender is null)
            return 0;
        var xd = Ender.X - Starter.X;
        var yd = Ender.Y - Starter.Y;
        return Math.Sqrt(xd * xd + yd * yd);
    }


#if DEBUG
    public override string ToString()
    {
        return (Starter?.ToString() ?? "NONE") + "->" + (Ender?.ToString() ?? "NONE");
    }

    public string ToString(string format)
    {
        return (Starter?.ToString(format) ?? "NONE") + "->" + (Ender?.ToString(format) ?? "NONE");
    }
#endif
}
