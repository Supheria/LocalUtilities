using LocalUtilities.VoronoiDiagram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.VoronoiDiagram;

internal class CornerBorderNode(VoronoiPoint point) : BorderNode
{
    public override Direction BorderLocation { get; } = point.BorderLocation;

    public override VoronoiPoint Point { get; } = point;

    public override double Angle => throw new InvalidOperationException();

    public override int FallbackComparisonIndex => throw new InvalidOperationException();

#if DEBUG
    public override string ToString()
    {
        return "Corner " + base.ToString();
    }
#endif
}
