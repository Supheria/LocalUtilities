using LocalUtilities.VoronoiDiagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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