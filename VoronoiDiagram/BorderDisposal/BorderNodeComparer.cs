using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.VoronoiDiagram;

internal class BorderNodeComparer : IComparer<BorderNode>
{
    public int Compare(BorderNode? n1, BorderNode? n2)
    {
        ArgumentNullException.ThrowIfNull(n1);
        ArgumentNullException.ThrowIfNull(n2);
        int locationCompare = DirectionOrder(n1.BorderLocation).CompareTo(DirectionOrder(n2.BorderLocation));
        if (locationCompare != 0)
            return locationCompare;
        return n1.BorderLocation switch // same for n2
        {
            // going up
            Direction.Left or Direction.LeftTop => NodeCompareTo(n1.Point.Y, n2.Point.Y, n1, n2, n1.BorderLocation),
            // going right
            Direction.Bottom or Direction.LeftBottom => NodeCompareTo(n1.Point.X, n2.Point.X, n1, n2, n1.BorderLocation),
            // going down
            Direction.Right or Direction.BottomRight => NodeCompareTo(n2.Point.Y, n1.Point.Y, n1, n2, n1.BorderLocation),
            // going left
            Direction.Top or Direction.TopRight => NodeCompareTo(n2.Point.X, n1.Point.X, n1, n2, n1.BorderLocation),
            _ => throw new InvalidOperationException(),
        };
    }

    private static int NodeCompareTo(double coord1, double coord2, BorderNode node1, BorderNode node2, Direction pointBorderLocation)
    {
        var comparison = coord1.ApproxCompareTo(coord2);
        if (comparison != 0)
            return comparison;
        var angleComparison = node1.CompareAngleTo(node2, pointBorderLocation);
        if (angleComparison != 0)
            return angleComparison;
        // Extremely unlikely, but just return something that sorts and doesn't equate
        var fallbackComparison = node1.FallbackComparisonIndex.CompareTo(node2.FallbackComparisonIndex);
        if (fallbackComparison != 0)
            return fallbackComparison;
        throw new InvalidOperationException(); // we should never get here if fallback index is proper
    }

    /// <remarks>
    /// Note that these are ordered counter-clockwise starting at bottom-left
    /// </remarks>
    private static int DirectionOrder(Direction direction)
    {
        return direction switch
        {
            Direction.LeftTop => 0,
            Direction.Left => 1,
            Direction.LeftBottom => 2,
            Direction.Bottom => 3,
            Direction.BottomRight => 4,
            Direction.Right => 5,
            Direction.TopRight => 6,
            Direction.Top => 7,
            _ => throw new InvalidOperationException(),
        };
    }
}