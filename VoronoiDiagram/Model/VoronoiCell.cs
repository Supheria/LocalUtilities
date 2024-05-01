namespace LocalUtilities.VoronoiDiagram.Model;

/// <summary>
/// The point/site/seed on the Voronoi plane.
/// This has a <see cref="Edges"/> of <see cref="VoronoiEdge"/>s.
/// This has <see cref="Vertices"/> of <see cref="VoronoiVertex"/>s that are the edge end points, i.e. the cell's vertices.
/// This also has <see cref="Neighbours"/>, i.e. <see cref="VoronoiCell"/>s across the <see cref="VoronoiEdge"/>s.
/// </summary>
public class VoronoiCell(double x, double y, int column, int row)
{
    public Coordinate Site { get; private set; } = new(x, y);

    public (int Column, int Row) Location { get; private set; } = (column, row);

    /// <summary>
    /// The edges that make up this cell.
    /// The vertices of these edges are the <see cref="Vertices"/>.
    /// These are also known as Thiessen polygons.
    /// </summary>
    internal List<VoronoiEdge> Edges { get; } = [];

    /// <summary>
    /// The sites across the edges.
    /// </summary>
    public List<VoronoiCell> Neighbours { get; } = [];

    /// <summary>
    /// The vertices of the <see cref="Edges"/>.
    /// </summary>
    public List<VoronoiVertex> Vertices
    {
        get
        {
            if (_vertices == null)
            {
                _vertices = [];
                var vertices = new Dictionary<(double X, double Y), Direction>();
                foreach (var edge in Edges)
                {
                    ArgumentNullException.ThrowIfNull(edge.End);
                    vertices[(edge.Start.X, edge.Start.Y)] = edge.Start.DirectionOnBorder;
                    vertices[(edge.End.X, edge.End.Y)] = edge.End.DirectionOnBorder;
                    // Note that .End is guaranteed to be set since we don't expose edges externally that aren't clipped in bounds
                }
                _vertices = vertices.Select(p => new VoronoiVertex(p.Key.X, p.Key.Y, p.Value)).ToList();
                _vertices.Sort(SortVerticesClockwisely);
            }
            return _vertices;
        }
    }
    List<VoronoiVertex>? _vertices = null;

    public Coordinate Centroid
    {
        get => _centroid ??= GetCentroid();
    }
    Coordinate? _centroid = null;

    public VoronoiCell() : this(0, 0, 0, 0)
    {

    }

    public bool ContainPoint(double x, double y)
    {
        // helper method to determine if a point is inside the cell
        // based on meowNET's answer from: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        bool result = false;
        int j = Vertices.Count - 1;
        for (int i = 0; i < Vertices.Count; i++)
        {
            if (Vertices[i].Y < y && Vertices[j].Y >= y ||
                Vertices[j].Y < y && Vertices[i].Y >= y)
            {
                if (Vertices[i].X + (y - Vertices[i].Y) /
                    (Vertices[j].Y - Vertices[i].Y) *
                    (Vertices[j].X - Vertices[i].X) < x)
                    result = !result;
            }
            j = i;
        }
        return result;
    }

    public bool ContainVertice(VoronoiVertex vertice)
    {
        foreach(var v in Vertices)
        {
            if (v.X.ApproxEqual(vertice.X) && v.Y.ApproxEqual(vertice.Y))
                return true;
        }
        return false;
    }

    public VoronoiVertex VerticeNeighbor(VoronoiVertex vertice, bool clockWise)
    {
        var index = 0;
        while (index < Vertices.Count)
        {
            var v = Vertices[index];
            if (v.X.ApproxEqual(vertice.X) && v.Y.ApproxEqual(vertice.Y))
            {
                if (clockWise)
                    index = (index + 1) % Vertices.Count;
                else
                    index = (index + Vertices.Count - 1) % Vertices.Count;
                return Vertices[index];
            }
            index++;
        }
        throw new ArgumentException();
    }

    /// <summary>
    /// If the site lies on any of the edges (or corners), then the starting order is not defined.
    /// </summary>
    private int SortVerticesClockwisely(VoronoiVertex point1, VoronoiVertex point2)
    {
        // When the point lies on top of us, we don't know what to use as an angle because that depends on which way the other edges "close".
        // So we "shift" the center a little towards the (approximate*) centroid of the polygon, which would "restore" the angle.
        // (* We don't want to waste time computing the actual true centroid though.)
        if (point1.ApproxEqual(Site.X, Site.Y) || point2.ApproxEqual(Site.X, Site.Y))
            return SortVerticesClockwisely(point1, point2, GetCenterShiftedX(), GetCenterShiftedY());
        return SortVerticesClockwisely(point1, point2, Site.X, Site.Y);
    }

    private static int SortVerticesClockwisely(VoronoiVertex point1, VoronoiVertex point2, double x, double y)
    {
        // originally, based on: https://social.msdn.microsoft.com/Forums/en-US/c4c0ce02-bbd0-46e7-aaa0-df85a3408c61/sorting-list-of-xy-coordinates-clockwise-sort-works-if-list-is-unsorted-but-fails-if-list-is?forum=csharplanguage
        // comparer to sort the array based on the points relative position to the center
        var atan1 = Atan2(point1.Y - y, point1.X - x);
        var atan2 = Atan2(point2.Y - y, point2.X - x);
        if (atan1 > atan2) return 1;
        if (atan1 < atan2) return -1;
        return 0;
    }

    private static double Atan2(double y, double x)
    {
        // "Normal" Atan2 returns an angle between -π ≤ θ ≤ π as "seen" on the Cartesian plane,
        // that is, starting at the "right" of x axis and increasing counter-clockwise.
        // But we want the angle sortable where the origin is the "lowest" angle: 0 ≤ θ ≤ 2×π
        var a = Math.Atan2(y, x);
        if (a < 0)
            a += 2 * Math.PI;
        return a;
    }

    /// <summary>
    /// the point of shifting coordinates is to "change the angle", 
    /// but Atan cannot distinguish anything smaller than something like double significant digits, 
    /// so we need this "epsilon" to be fairly large
    /// </summary>
    const double shiftAmount = 1 / 1E14;

    private double GetCenterShiftedX()
    {
        var target = Edges.Sum(c => c.Start.X + c.End?.X) / Edges.Count / 2;
        return Site.X + (target - Site.X) * shiftAmount ?? throw new ArgumentNullException();
    }

    private double GetCenterShiftedY()
    {
        var target = Edges.Sum(c => c.Start.Y + c.End?.Y) / Edges.Count / 2;
        return Site.Y + (target - Site.Y) * shiftAmount ?? throw new ArgumentNullException();
    }

    /// <summary>
    /// The center of our cell.
    /// Specifically, the geometric center aka center of mass, i.e. the arithmetic mean position of all the edge end points.
    /// This is assuming a non-self-intersecting closed polygon of our cell.
    /// If we don't have a closed cell (i.e. unclosed "polygon"), then this will produce approximate results that aren't mathematically sound, but work for most purposes. 
    /// </summary>
    private Coordinate GetCentroid()
    {
        // Basically, https://stackoverflow.com/a/34732659
        // https://en.wikipedia.org/wiki/Centroid#Of_a_polygon
        // If we don't have points generated yet, do so now (by calling the property that does so when read)

        // Cx = (1 / 6A) * ∑ (x1 + x2) * (x1 * y2 - x2 + y1)
        // Cy = (1 / 6A) * ∑ (y1 + y2) * (x1 * y2 - x2 + y1)
        // A = (1 / 2) * ∑ (x1 * y2 - x2 * y1)
        // where x2/y2 is next point after x1/y1, including looping last
        double centroidX = 0; // just for compiler to be happy, we won't use these default values
        double centroidY = 0;
        double area = 0;
        for (int i = 0; i < Vertices.Count; i++)
        {
            int i2 = i == Vertices.Count - 1 ? 0 : i + 1;
            double xi = Vertices[i].X;
            double yi = Vertices[i].Y;
            double xi2 = Vertices[i2].X;
            double yi2 = Vertices[i2].Y;
            double mult = (xi * yi2 - xi2 * yi) / 3;
            // Second multiplier is the same for both x and y, so "extract"
            // Also since C = 1/(6A)... and A = (1/2)..., we can just apply the /3 divisor here to not lose precision on large numbers 
            double addX = (xi + xi2) * mult;
            double addY = (yi + yi2) * mult;
            double addArea = xi * yi2 - xi2 * yi;
            if (i == 0)
            {
                centroidX = addX;
                centroidY = addY;
                area = addArea;
            }
            else
            {
                centroidX += addX;
                centroidY += addY;
                area += addArea;
            }
        }
        // If the area is 0, then we are basically squashed on top of other points... weird, but ok, this makes centroid exactly us
        if (area.ApproxEqual(0))
            return Site;
        centroidX /= area;
        centroidY /= area;
        return new(centroidX, centroidY);
    }

    public Rectangle GetBounds()
    {
        if (Vertices.Count is 0)
            return new(0, 0, 0, 0);
        double left, right, top, bottom;
        left = right = Vertices[0].X;
        top = bottom = Vertices[0].Y;
        for (int i = 1; i < Vertices.Count; i++)
        {
            var point = Vertices[i];
            left = Math.Min(left, point.X);
            right = Math.Max(right, point.X);
            top = Math.Min(top, point.Y);
            bottom = Math.Max(bottom, point.Y);
        }
        return new((int)left, (int)top, (int)(right - left), (int)(bottom - top));
    }

    public double GetArea()
    {
        var count = Vertices.Count;
        if (count < 3)
            return 0.0;
        double s = Vertices[0].Y * (Vertices[count - 1].X - Vertices[1].X);
        for (int i = 1; i < count; ++i)
            s += Vertices[i].Y * (Vertices[i - 1].X - Vertices[(i + 1) % count].X);
        return Math.Abs(s / 2.0);
    }

#if DEBUG
    public override string ToString()
    {
        return $"{Centroid.X:F3}, {Centroid.Y:F3} ({Location.Column},{Location.Row})";
    }
#endif
}
