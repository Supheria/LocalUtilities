namespace LocalUtilities.VoronoiDiagram.Model;

/// <summary>
/// The point/site/seed on the Voronoi plane.
/// This has a <see cref="CellEdges"/> of <see cref="VoronoiEdge"/>s.
/// This has <see cref="CellVertices"/> of <see cref="VoronoiPoint"/>s that are the edge end points, i.e. the cell's vertices.
/// This also has <see cref="Neighbours"/>, i.e. <see cref="VoronoiCell"/>s across the <see cref="VoronoiEdge"/>s.
/// </summary>
public class VoronoiCell(double x, double y)
{
    public VoronoiPoint Site { get; private set; } = new(x, y);

    /// <summary>
    /// The edges that make up this cell.
    /// The vertices of these edges are the <see cref="CellVertices"/>.
    /// These are also known as Thiessen polygons.
    /// </summary>
    internal List<VoronoiEdge> CellEdges { get; } = [];

    /// <summary>
    /// The sites across the edges.
    /// </summary>
    public List<VoronoiCell> Neighbours { get; } = [];

    /// <summary>
    /// The vertices of the <see cref="CellEdges"/>.
    /// </summary>
    public List<VoronoiPoint> CellVertices
    {
        get
        {
            if (_cellVertices == null)
            {
                _cellVertices = [];
                var vertices = new Dictionary<(double X, double Y), Direction>();
                foreach (var edge in CellEdges)
                {
                    ArgumentNullException.ThrowIfNull(edge.End);
                    vertices[(edge.Start.X, edge.Start.Y)] = edge.Start.BorderLocation;
                    vertices[(edge.End.X, edge.End.Y)] = edge.End.BorderLocation;
                    // Note that .End is guaranteed to be set since we don't expose edges externally that aren't clipped in bounds
                }
                _cellVertices = vertices.Select(p => new VoronoiPoint(p.Key.X, p.Key.Y, p.Value)).ToList();
                _cellVertices.Sort(SortCellVerticesClockwisely);
            }
            return _cellVertices;
        }
    }
    List<VoronoiPoint>? _cellVertices = null;

    public VoronoiPoint Centroid
    {
        get => _centroid ??= GetCentroid();
    }
    VoronoiPoint? _centroid = null;

    public VoronoiCell() : this(0, 0)
    {

    }

    internal void Relocate(double newX, double newY)
    {
        Site = new(newX, newY);
        CellEdges.Clear();
        Neighbours.Clear();
        _cellVertices = null;
    }

    public bool Contains(double x, double y)
    {
        // helper method to determine if a point is inside the cell
        // based on meowNET's answer from: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        bool result = false;
        int j = CellVertices.Count - 1;
        for (int i = 0; i < CellVertices.Count; i++)
        {
            if (CellVertices[i].Y < y && CellVertices[j].Y >= y ||
                CellVertices[j].Y < y && CellVertices[i].Y >= y)
            {
                if (CellVertices[i].X + (y - CellVertices[i].Y) /
                    (CellVertices[j].Y - CellVertices[i].Y) *
                    (CellVertices[j].X - CellVertices[i].X) < x)
                    result = !result;
            }
            j = i;
        }
        return result;
    }

    /// <summary>
    /// If the site lies on any of the edges (or corners), then the starting order is not defined.
    /// </summary>
    private int SortCellVerticesClockwisely(VoronoiPoint point1, VoronoiPoint point2)
    {
        // When the point lies on top of us, we don't know what to use as an angle because that depends on which way the other edges "close".
        // So we "shift" the center a little towards the (approximate*) centroid of the polygon, which would "restore" the angle.
        // (* We don't want to waste time computing the actual true centroid though.)
        if (point1.ApproxEqual(Site.X, Site.Y) || point2.ApproxEqual(Site.X, Site.Y))
            return SortCellVerticesClockwisely(point1, point2, GetCenterShiftedX(), GetCenterShiftedY());
        return SortCellVerticesClockwisely(point1, point2, Site.X, Site.Y);
    }

    private static int SortCellVerticesClockwisely(VoronoiPoint point1, VoronoiPoint point2, double x, double y)
    {
        // originally, based on: https://social.msdn.microsoft.com/Forums/en-US/c4c0ce02-bbd0-46e7-aaa0-df85a3408c61/sorting-list-of-xy-coordinates-clockwise-sort-works-if-list-is-unsorted-but-fails-if-list-is?forum=csharplanguage
        // comparer to sort the array based on the points relative position to the center
        var atan1 = Atan2(point1.Y - y, point1.X - x);
        var atan2 = Atan2(point2.Y - y, point2.X - x);
        if (atan1 > atan2) return -1;
        if (atan1 < atan2) return 1;
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
        var target = CellEdges.Sum(c => c.Start.X + c.End?.X) / CellEdges.Count / 2;
        return Site.X + (target - Site.X) * shiftAmount ?? throw new ArgumentNullException();
    }

    private double GetCenterShiftedY()
    {
        var target = CellEdges.Sum(c => c.Start.Y + c.End?.Y) / CellEdges.Count / 2;
        return Site.Y + (target - Site.Y) * shiftAmount ?? throw new ArgumentNullException();
    }

    /// <summary>
    /// The center of our cell.
    /// Specifically, the geometric center aka center of mass, i.e. the arithmetic mean position of all the edge end points.
    /// This is assuming a non-self-intersecting closed polygon of our cell.
    /// If we don't have a closed cell (i.e. unclosed "polygon"), then this will produce approximate results that aren't mathematically sound, but work for most purposes. 
    /// </summary>
    private VoronoiPoint GetCentroid()
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
        for (int i = 0; i < CellVertices.Count; i++)
        {
            int i2 = i == CellVertices.Count - 1 ? 0 : i + 1;
            double xi = CellVertices[i].X;
            double yi = CellVertices[i].Y;
            double xi2 = CellVertices[i2].X;
            double yi2 = CellVertices[i2].Y;
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
        if (CellVertices.Count is 0)
            return new(0, 0, 0, 0);
        double left, right, top, bottom;
        left = right = CellVertices[0].X;
        top = bottom = CellVertices[0].Y;
        for (int i = 1; i < CellVertices.Count; i++)
        {
            var point = CellVertices[i];
            left = Math.Min(left, point.X);
            right = Math.Max(right, point.X);
            top = Math.Min(top, point.Y);
            bottom = Math.Max(bottom, point.Y);
        }
        return new((int)left, (int)top, (int)(right - left), (int)(bottom - top));
    }

    public double GetArea()
    {
        var count = CellVertices.Count;
        if (count < 3)
            return 0.0;
        double s = CellVertices[0].Y * (CellVertices[count - 1].X - CellVertices[1].X);
        for (int i = 1; i < count; ++i)
            s += CellVertices[i].Y * (CellVertices[i - 1].X - CellVertices[(i + 1) % count].X);
        return Math.Abs(s / 2.0);
    }

#if DEBUG
    public override string ToString()
    {
        return "(" + Site.X.ToString("F3") + "," + Site.Y.ToString("F3") + ")";
    }
#endif
}
