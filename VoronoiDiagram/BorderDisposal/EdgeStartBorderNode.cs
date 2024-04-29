using LocalUtilities.VoronoiDiagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.VoronoiDiagram;

internal class EdgeStartBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
        EdgeBorderNode(edge, fallbackComparisonIndex)
{
    public override Direction BorderLocation => Edge.Start.BorderLocation;

    public override VoronoiPoint Point => Edge.Start;

    public override double Angle => Point.AngleTo(Edge.End); // away from border

#if DEBUG
    public override string ToString()
    {
        return "Edge Start " + base.ToString();
    }
#endif
}