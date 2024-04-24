using LocalUtilities.GdiUtilities.Utilities;
using LocalUtilities.GdiUtilities.VoronoiDiagram.FortuneAlgorithm;
using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace VoronoiDiagram;

/// <summary>
/// An Euclidean plane where a Voronoi diagram can be constructed from <see cref="VoronoiSite"/>s
/// producing a tesselation of cells with <see cref="VoronoiEdge"/> line segments and <see cref="VoronoiPoint"/> vertices.
/// </summary>
public class VoronoiPlane(double minX, double minY, double maxX, double maxY)
{
    public List<VoronoiSite> Sites { get; set; } = [];

    public List<VoronoiEdge> Edges { get; private set; } = [];

    public double MinX { get; } = minX != maxX ? Math.Min(minX, maxX) : throw new ArgumentException();

    public double MinY { get; } = minY != maxY ? Math.Min(minY, maxY) : throw new ArgumentException();

    public double MaxX { get; } = minX != maxX ? Math.Max(minX, maxX) : throw new ArgumentException();

    public double MaxY { get; } = minY != maxY ? Math.Max(minY, maxY) : throw new ArgumentException();

    bool GenerateBorder { get; set; } = true;

    /// <summary>
    /// The generated sites are guaranteed not to lie on the border of the plane (although they may be very close).
    /// </summary>

    public List<VoronoiSite> GenerateRandomSites(int amount, PointGenerationMethod method)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Func<PointGenerationMethod, RandomPointGeneration> getGeneration = method =>
        {
            return method switch
            {
                PointGenerationMethod.Uniform => new RandomUniformPointGeneration(),
                PointGenerationMethod.Gaussian => new RandomGaussianPointGeneration(),
                _ => throw new ArgumentOutOfRangeException()
            };
        };
        var algorithm = getGeneration(method);
        Sites = algorithm.Generate(MinX, MinY, MaxX, MaxY, amount);
        Edges.Clear();
        return Sites;
    }

    public List<VoronoiEdge> Tessellate(bool generateBorder)
    {
        GenerateBorder = generateBorder;
        if (Sites.Count is 0) 
            throw new VoronoiException();
        List<VoronoiEdge> edges = new FortunesTessellation().Run(Sites, MinX, MinY, MaxX, MaxY);
        edges = new BorderClipping().Clip(edges, MinX, MinY, MaxX, MaxY);
        if (generateBorder)
            edges = new BorderClosing().Close(edges, MinX, MinY, MaxX, MaxY, Sites);
        Edges = edges;
        return Edges;
    }


    public List<VoronoiEdge> Relax(int iterations = 1, float strength = 1.0f, bool reTessellate = true)
    {
        VoronoiException.ThrowIfSitesCountIsZero(Sites);
        VoronoiException.ThrowIfNotTessellated(Edges);
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        if (strength <= 0f || strength > 1f) 
            throw new ArgumentOutOfRangeException(nameof(strength));
        for (int i = 0; i < iterations; i++)
        {
            new LloydsRelaxation().Relax(Sites, MinX, MinY, MaxX, MaxY, strength);
            if (reTessellate)
                Tessellate(GenerateBorder);
        }
        return Edges;
    }

    public List<VoronoiSite> MergeSites(VoronoiSiteMergeQuery mergeQuery)
    {
        VoronoiException.ThrowIfSitesCountIsZero(Sites);
        VoronoiException.ThrowIfNotTessellated(Edges);
        ArgumentNullException.ThrowIfNull(mergeQuery);
        new GenericSiteMergingAlgorithm().MergeSites(Sites, Edges, mergeQuery);
        return Sites;
    }
}
