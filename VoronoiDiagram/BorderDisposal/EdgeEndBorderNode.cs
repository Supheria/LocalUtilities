using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

internal class EdgeEndBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
        EdgeBorderNode(edge, fallbackComparisonIndex)
{
    public override Direction BorderLocation => Edge.End.BorderLocation;

    public override VoronoiPoint Point => Edge.End;

    public override double Angle => Point.AngleTo(Edge.Start); // away from border


#if DEBUG
    public override string ToString()
    {
        return "Edge End " + base.ToString();
    }
#endif
}