using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

internal class EdgeEndBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
        EdgeBorderNode(edge, fallbackComparisonIndex)
{
    public override Direction BorderLocation => Edge.Ender.DirectionOnBorder;

    public override VoronoiVertex Vertex => Edge.Ender;

    public override double Angle => Vertex.AngleTo(Edge.Starter); // away from border


#if DEBUG
    public override string ToString()
    {
        return "Edge End " + base.ToString();
    }
#endif
}