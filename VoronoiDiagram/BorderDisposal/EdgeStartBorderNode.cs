using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

internal class EdgeStartBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
        EdgeBorderNode(edge, fallbackComparisonIndex)
{
    public override Direction BorderLocation => Edge.Start.DirectionOnBorder;

    public override VoronoiVertex Point => Edge.Start;

    public override double Angle => Point.AngleTo(Edge.End); // away from border

#if DEBUG
    public override string ToString()
    {
        return "Edge Start " + base.ToString();
    }
#endif
}