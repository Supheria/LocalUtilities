using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

public class VoronoiException : Exception
{
    public static void ThrowIfCountZero(List<VoronoiCell> voronoiSites)
    {
        if (voronoiSites.Count is 0)
            throw new("This data is not ready yet, you must add sites to the plane first.");
    }

    public static void ThrowIfCountZero(List<VoronoiEdge> voronoiEdges)
    {
        if (voronoiEdges.Count is 0)
            throw new("This data is not ready yet, you must tessellate the plane first.");
    }
}