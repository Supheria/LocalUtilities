using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace LocalUtilities.GdiUtilities.Utilities;

public class VoronoiException : Exception
{
    public static void ThrowIfSitesCountIsZero(List<VoronoiSite> voronoiSites)
    {
        if (voronoiSites.Count is 0)
            throw new("This data is not ready yet, you must add sites to the plane first.");
    }

    public static void ThrowIfNotTessellated(List<VoronoiEdge> voronoiEdges)
    {
        if (voronoiEdges.Count is 0)
            throw new("This data is not ready yet, you must tessellate the plane first.");
    }
}