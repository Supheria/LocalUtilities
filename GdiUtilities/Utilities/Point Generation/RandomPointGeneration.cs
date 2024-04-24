using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace VoronoiDiagram;

public enum PointGenerationMethod
{
    Uniform,
    Gaussian
}

internal abstract class RandomPointGeneration
{
    public List<VoronoiSite> Generate(double minX, double minY, double maxX, double maxY, int count)
    {
        List<VoronoiSite> sites = new List<VoronoiSite>(count);

        Random random = new Random();

        for (int i = 0; i < count; i++)
        {
            sites.Add(
                new VoronoiSite(
                    GetNextRandomValue(random, minX, maxX),
                    GetNextRandomValue(random, minY, maxY)
                )
            );
        }

        return sites;
    }

    protected abstract double GetNextRandomValue(Random random, double min, double max);
}