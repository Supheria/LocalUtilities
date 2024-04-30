using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

internal class CornerBorderNode(VoronoiVertice point) : BorderNode
{
    public override Direction BorderLocation { get; } = point.BorderLocation;

    public override VoronoiVertice Point { get; } = point;

    public override double Angle => throw new InvalidOperationException();

    public override int FallbackComparisonIndex => throw new InvalidOperationException();

#if DEBUG
    public override string ToString()
    {
        return "Corner " + base.ToString();
    }
#endif
}
