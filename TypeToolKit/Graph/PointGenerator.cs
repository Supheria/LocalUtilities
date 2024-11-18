namespace LocalUtilities.TypeToolKit.Graph;

public class PointGenerator
{
    public static List<(double X, double Y)> GeneratePoint(Random random, double minX, double minY, double maxX, double maxY, int count)
    {
        var sites = new List<(double X, double Y)>(count);
        for (int i = 0; i < count; i++)
        {
            sites.Add(new(
                nextGaussianRandom(random, minX, maxX),
                nextGaussianRandom(random, minY, maxY)
                ));
        }
        return sites;
        double nextGaussianRandom(Random random, double min, double max)
        {
            // Box-Muller transform
            // From: https://stackoverflow.com/a/218600
            const double stdDev = 1.0 / 3.0; // this covers 99.73% of cases in (-1..1) range
            var mid = (max + min) / 2;
            do
            {
                var u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
                var u2 = 1.0 - random.NextDouble();
                var randStdNormal =
                    Math.Sqrt(-2.0 * Math.Log(u1)) *
                    Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double value = stdDev * randStdNormal;
                double coord = mid + value * mid;
                if (coord > min && coord < max)
                    return coord;
            } while (true);
        }
    }
}
