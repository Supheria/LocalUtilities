namespace LocalUtilities.GdiUtilities;

public abstract class RandomPointsGeneration : IPointsGeneration
{
    public List<(double X, double Y)> Generate(double minX, double minY, double maxX, double maxY, int count)
    {
        var sites = new List<(double X, double Y)>(count);
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            sites.Add(new(
                GetNextRandomValue(random, minX, maxX),
                GetNextRandomValue(random, minY, maxY)
                ));
        }
        return sites;
    }

    protected abstract double GetNextRandomValue(Random random, double min, double max);
}