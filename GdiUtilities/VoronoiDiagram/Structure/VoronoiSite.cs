﻿using LocalUtilities.GdiUtilities.Utilities;
using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

/// <summary>
/// The point/site/seed on the Voronoi plane.
/// This has a <see cref="CellEdges"/> of <see cref="VoronoiEdge"/>s.
/// This has <see cref="CellVertices"/> of <see cref="VoronoiPoint"/>s that are the edge end points, i.e. the cell's vertices.
/// This also has <see cref="Neighbours"/>, i.e. <see cref="VoronoiSite"/>s across the <see cref="VoronoiEdge"/>s.
/// </summary>
public class VoronoiSite(double x, double y)
{
    public double X { get; private set; } = x;

    public double Y { get; private set; } = y;

    /// <summary>
    /// The edges that make up this cell.
    /// The vertices of these edges are the <see cref="CellVertices"/>.
    /// These are also known as Thiessen polygons.
    /// </summary>
    public List<VoronoiEdge> CellEdges { get; } = [];

    /// <summary>
    /// The sites across the edges.
    /// </summary>
    public List<VoronoiSite> Neighbours { get; } = [];

    /// <summary>
    /// The vertices of the <see cref="CellEdges"/>.
    /// </summary>
    public List<VoronoiPoint> CellVertices
    {
        get
        {
            if (_cellVertices == null)
            {
                _cellVertices = new();
                var vertices = new Dictionary<(double X, double Y), PointBorderLocation>();
                foreach (var edge in CellEdges)
                {
                    vertices[(edge.Start.X, edge.Start.Y)] = edge.Start.BorderLocation;
                    vertices[(edge.End.X, edge.End.Y)] = edge.End.BorderLocation;
                    // Note that .End is guaranteed to be set since we don't expose edges externally that aren't clipped in bounds
                }
                _cellVertices = vertices.Select(p => new VoronoiPoint(p.Key.X, p.Key.Y, p.Value)).ToList();
            }
            return _cellVertices;
        }
    }
    private List<VoronoiPoint>? _cellVertices = null;

    /// <summary>
    /// Whether this site lies directly on exactly one of its <see cref="CellEdges"/>'s edges.
    /// This happens when sites overlap or are on the border.
    /// This won't be set if instead <see cref="LiesOnCorner"/> is set, i.e. the site lies on the intersection of 2 of its edges.
    /// </summary>
    public VoronoiEdge? LiesOnEdge { get; private set; } = null;

    /// <summary>
    /// Whether this site lies directly on the intersection point of two of its <see cref="CellEdges"/>'s edges.
    /// This happens when sites overlap or are on the border's corner.
    /// </summary>
    public VoronoiPoint? LiesOnCorner { get; private set; } = null;

    public bool Contains(double x, double y)
    {
        // helper method to determine if a point is inside the cell
        // based on meowNET's answer from: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        var vertices = ClockwiseCellVertices();
        bool result = false;
        int j = vertices.Count - 1;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].Y < y && vertices[j].Y >= y ||
                vertices[j].Y < y && vertices[i].Y >= y)
            {
                if (vertices[i].X + (y - vertices[i].Y) /
                    (vertices[j].Y - vertices[i].Y) *
                    (vertices[j].X - vertices[i].X) < x)
                    result = !result;
            }
            j = i;
        }
        return result;
    }

    internal void AddEdge(VoronoiEdge newEdge)
    {
        CellEdges.Add(newEdge);
        // Set the "flags" whether we are on an edge or corner
        if (LiesOnCorner != null)
            return; // we already are on a corner, we cannot be on 2 corners, so no need to check anything
        if (!DoesLieOnEdge(newEdge))
            return; // we are not on this edge - no changes needed
        if (LiesOnEdge is null)
            LiesOnEdge = newEdge;
        else
        {
            // We are already on an edge, so this must be the second edge, i.e. we lie on the corner
            if (newEdge.Start == LiesOnEdge.Start || newEdge.Start == LiesOnEdge.End)
                LiesOnCorner = newEdge.Start;
            else
                LiesOnCorner = newEdge.End;
            LiesOnEdge = null; // we only keep this for one and only one edge
        }
    }

    private bool DoesLieOnEdge(VoronoiEdge edge)
    {
        return PointsAreColinear(
            X, Y,
            edge.Start.X, edge.Start.Y,
            edge.End.X, edge.End.Y
        );
    }

    private static bool PointsAreColinear(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        // Based off https://stackoverflow.com/a/328110
        // Cross product 2-1 x 3-1
        return ((x2 - x1) * (y3 - y1)).ApproxEqual((x3 - x1) * (y2 - y1));
    }

    internal void Relocate(double newX, double newY)
    {
        X = newX;
        Y = newY;
        CellEdges.Clear();
        Neighbours.Clear();
        _cellVertices = null;
        LiesOnEdge = null;
        LiesOnCorner = null;
    }

    /// <summary>
    /// If the site lies on any of the edges (or corners), then the starting order is not defined.
    /// </summary>
    public List<VoronoiPoint> ClockwiseCellVertices()
    {
        CellVertices.Sort(SortCellVerticesClockwisely);
        return CellVertices;
    }

    private int SortCellVerticesClockwisely(VoronoiPoint point1, VoronoiPoint point2)
    {
        // When the point lies on top of us, we don't know what to use as an angle because that depends on which way the other edges "close".
        // So we "shift" the center a little towards the (approximate*) centroid of the polygon, which would "restore" the angle.
        // (* We don't want to waste time computing the actual true centroid though.)

        if (point1.ApproxEqual(X, Y) ||
            point2.ApproxEqual(X, Y))
            return SortCellVerticesClockwisely(point1, point2, GetCenterShiftedX(), GetCenterShiftedY());

        return SortCellVerticesClockwisely(point1, point2, X, Y);
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
    /// If the site lies on any of the edges (or corners), then the starting order is not defined.
    /// </summary>
    public List<VoronoiEdge> ClockwiseCellEdges()
    {
        CellEdges.Sort(SortCellEdgesClockwisely);
        return CellEdges;
    }

    private int SortCellEdgesClockwisely(VoronoiEdge edge1, VoronoiEdge edge2)
    {
        int result;
        if (DoesLieOnEdge(edge1) || DoesLieOnEdge(edge2))
        {
            // If we are on either edge then we can't compare directly to that edge,
            // because angle to the edge is basically "along the edge", i.e. undefined.
            // We don't know which "direction" the cell will turn, we don't know if the cell is to the right/or left of the edge.
            // So we "step away" a little bit towards out cell's/polygon's center so that we are no longer on either edge.
            // This means we can now get the correct angle, which is slightly different now, but all we care about is the origin/quadrant.
            // This is a roundabout way to do this, but it seems to work well enough.
            var centerX = GetCenterShiftedX();
            var centerY = GetCenterShiftedY();
            if (EdgeCrossesOrigin(edge1, centerX, centerY))
                result = 1; // this makes edge 1 the last edge among all (cell's) edges
            else if (EdgeCrossesOrigin(edge2, centerX, centerY))
                result = -1; // this makes edge 2 the last edge among all (cell's) edges
            else
                result = SortCellVerticesClockwisely(edge1.GetMid(), edge2.GetMid(), centerX, centerY);
        }
        else
        {
            if (EdgeCrossesOrigin(edge1))
                result = 1; // this makes edge 1 the last edge among all (cell's) edges
            else if (EdgeCrossesOrigin(edge2))
                result = -1; // this makes edge 2 the last edge among all (cell's) edges
            else
                result = SortCellVerticesClockwisely(edge1.GetMid(), edge2.GetMid(), X, Y);
        }
        return result;
        // Note that we don't assume that edges connect.
    }

    /// <summary>
    /// the point of shifting coordinates is to "change the angle", 
    /// but Atan cannot distinguish anything smaller than something like double significant digits, 
    /// so we need this "epsilon" to be fairly large
    /// </summary>
    const double shiftAmount = 1 / 1E14;

    private double GetCenterShiftedX()
    {
        var target = CellEdges.Sum(c => c.Start.X + c.End.X) / CellEdges.Count / 2;
        return X + (target - X) * shiftAmount;
    }

    private double GetCenterShiftedY()
    {
        var target = CellEdges.Sum(c => c.Start.Y + c.End.Y) / CellEdges.Count / 2;
        return Y + (target - Y) * shiftAmount;
    }

    private bool EdgeCrossesOrigin(VoronoiEdge edge)
    {
        double atanA = Atan2(edge.Start.Y - Y, edge.Start.X - X);
        double atanB = Atan2(edge.End!.Y - Y, edge.End.X - X);
        // Edge can only "cover" less than half the circle by definition, otherwise then it wouldn't actually "contain" the site
        // So when the difference between end point angles is greater than half a circle, we know we have and edge that "crossed" the angle origin.
        return Math.Abs(atanA - atanB) > Math.PI;
    }

    private static bool EdgeCrossesOrigin(VoronoiEdge edge, double originX, double originY)
    {
        double atanA = Atan2(edge.Start.Y - originY, edge.Start.X - originX);
        double atanB = Atan2(edge.End!.Y - originY, edge.End.X - originX);
        // Edge can only "cover" less than half the circle by definition, otherwise then it wouldn't actually "contain" the site
        // So when the difference between end point angles is greater than half a circle, we know we have and edge that "crossed" the angle origin.
        return Math.Abs(atanA - atanB) > Math.PI;
    }

    /// <summary>
    /// The center of our cell.
    /// Specifically, the geometric center aka center of mass, i.e. the arithmetic mean position of all the edge end points.
    /// This is assuming a non-self-intersecting closed polygon of our cell.
    /// If we don't have a closed cell (i.e. unclosed "polygon"), then this will produce approximate results that aren't mathematically sound, but work for most purposes. 
    /// </summary>
    public VoronoiPoint GetCentroid()
    {
        // Basically, https://stackoverflow.com/a/34732659
        // https://en.wikipedia.org/wiki/Centroid#Of_a_polygon
        // If we don't have points generated yet, do so now (by calling the property that does so when read)

        // Cx = (1 / 6A) * ∑ (x1 + x2) * (x1 * y2 - x2 + y1)
        // Cy = (1 / 6A) * ∑ (y1 + y2) * (x1 * y2 - x2 + y1)
        // A = (1 / 2) * ∑ (x1 * y2 - x2 * y1)
        // where x2/y2 is next point after x1/y1, including looping last
        var vertices = ClockwiseCellVertices();
        double centroidX = 0; // just for compiler to be happy, we won't use these default values
        double centroidY = 0;
        double area = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            int i2 = i == vertices.Count - 1 ? 0 : i + 1;
            double xi = vertices[i].X;
            double yi = vertices[i].Y;
            double xi2 = vertices[i2].X;
            double yi2 = vertices[i2].Y;
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
            return new(X, Y);
        centroidX /= area;
        centroidY /= area;
        return new(centroidX, centroidY);
    }

    internal void InvalidateComputedValues()
    {
        _cellVertices = null;
    }

    internal void Invalidated()
    {
        CellEdges.Clear(); // don't cling to any references
        Neighbours.Clear(); // don't cling to any references
        _cellVertices = null;
    }

#if DEBUG
    public override string ToString()
    {
        return "(" + X.ToString("F3") + "," + Y.ToString("F3") + ")";
    }
#endif
}