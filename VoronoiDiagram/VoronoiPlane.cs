using LocalUtilities.GdiUtilities;
using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

/// <summary>
/// An Euclidean plane where a Voronoi diagram can be constructed from <see cref="VoronoiCell"/>s
/// producing a tesselation of cells with <see cref="VoronoiEdge"/> line segments and <see cref="VoronoiPoint"/> vertices.
/// </summary>
public class VoronoiPlane(double minX, double minY, double maxX, double maxY)
{
    public List<VoronoiCell> Cells { get; private set; } = [];

    public List<VoronoiEdge> Edges { get; private set; } = [];

    public double MinX { get; } = minX != maxX ? Math.Min(minX, maxX) : throw new ArgumentException();

    public double MinY { get; } = minY != maxY ? Math.Min(minY, maxY) : throw new ArgumentException();

    public double MaxX { get; } = minX != maxX ? Math.Max(minX, maxX) : throw new ArgumentException();

    public double MaxY { get; } = minY != maxY ? Math.Max(minY, maxY) : throw new ArgumentException();

    bool GenerateBorder { get; set; } = true;

    public void Generate(List<(double X, double Y)> points)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points.Count);
        Cells = UniquePoints(points).Select(p => new VoronoiCell(p.X, p.Y)).ToList();
        Edges.Clear();
        Generate();
    }

    /// <summary>
    /// The generated sites are guaranteed not to lie on the border of the plane (although they may be very close).
    /// </summary>
    public void Generate(int count, IPointsGeneration pointsGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        var remain = count;
        do
        {
            Cells = UniquePoints(pointsGeneration.Generate(MinX, MinY, MaxX, MaxY, remain))
                .Select(p => new VoronoiCell(p.X, p.Y)).ToList();
            remain = count - Cells.Count;
        } while (remain > 0);
        Edges.Clear();
        Generate();
    }

    private static List<(double X, double Y)> UniquePoints(List<(double X, double Y)> points)
    {
        points.Sort((p1, p2) =>
        {
            if (p1.X.ApproxEqual(p2.X))
            {
                if (p1.Y.ApproxEqual(p2.Y))
                    return 0;
                if (p1.Y < p2.Y)
                    return -1;
                return 1;
            }
            if (p1.X < p2.X)
                return -1;
            return 1;
        });
        var unique = new List<(double X, double Y)>();
        var last = points.First();
        unique.Add(last);
        for (var index = 1; index < points.Count; index++)
        {
            var point = points[index];
            if (!last.X.ApproxEqual(point.X) ||
                !last.Y.ApproxEqual(point.Y))
            {
                unique.Add(point);
                last = point;
            }
        }
        return unique;
    }

    private void Generate()
    {
        var eventQueue = new MinHeap<IFortuneEvent>(5 * Cells.Count);
        foreach (var site in Cells)
            eventQueue.Insert(new FortuneSiteEvent(site));
        //init tree
        var beachLine = new BeachLine();
        var edges = new LinkedList<VoronoiEdge>();
        var deleted = new HashSet<FortuneCircleEvent>();
        //init edge list
        while (eventQueue.Count != 0)
        {
            IFortuneEvent fEvent = eventQueue.Pop();
            if (fEvent is FortuneSiteEvent)
                beachLine.AddBeachSection((FortuneSiteEvent)fEvent, eventQueue, deleted, edges);
            else
            {
                if (deleted.Contains((FortuneCircleEvent)fEvent))
                    deleted.Remove((FortuneCircleEvent)fEvent);
                else
                    beachLine.RemoveBeachSection((FortuneCircleEvent)fEvent, eventQueue, deleted, edges);
            }
        }
        Edges = edges.ToList();
        Edges = BorderClipping.Clip(Edges, MinX, MinY, MaxX, MaxY);
        if (GenerateBorder)
            Edges = BorderClosing.Close(Edges, MinX, MinY, MaxX, MaxY, Cells);
    }

    public void RelaxSites(int iterations = 1, float strength = 1.0f)
    {
        VoronoiException.ThrowIfCountZero(Edges);
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        if (strength <= 0f || strength > 1f)
            throw new ArgumentOutOfRangeException(nameof(strength));
        for (int i = 0; i < iterations; i++)
        {
            var fullStrength = Math.Abs(strength - 1.0f) < float.Epsilon;
            foreach (var cell in Cells)
            {
                var centroid = cell.Centroid;
                if (fullStrength)
                    cell.Relocate(centroid.X, centroid.Y);
                else
                {
                    var site = cell.Site;
                    var newX = site.X + (centroid.X - site.X) * strength;
                    var newY = site.Y + (centroid.Y - site.Y) * strength;
                    cell.Relocate(newX, newY);
                }
            }
            Generate();
        }
    }
}
