using LocalUtilities.VoronoiDiagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.VoronoiDiagram;

internal abstract class EdgeBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) : BorderNode
{
    public VoronoiEdge Edge { get; } = edge;

    public override int FallbackComparisonIndex { get; } = fallbackComparisonIndex;
}
