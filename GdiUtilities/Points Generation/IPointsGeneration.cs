namespace LocalUtilities.GdiUtilities;

public interface IPointsGeneration
{
    public List<(double X, double Y)> Generate(double minX, double minY, double maxX, double maxY, int count);
}