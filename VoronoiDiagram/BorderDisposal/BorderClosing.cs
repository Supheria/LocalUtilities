using LocalUtilities.VoronoiDiagram.Model;

namespace LocalUtilities.VoronoiDiagram;

internal class BorderClosing
{
    public List<VoronoiEdge> Close(List<VoronoiEdge> edges, double minX, double minY, double maxX, double maxY, List<VoronoiCell> cells)
    {
        // We construct edges in clockwise order on the border:
        // →→→→→→→→↓
        // ↑       ↓
        // ↑       ↓
        // ↑       ↓
        // O←←←←←←←←

        // We construct edges between nodes on this border.
        // Nodes are points that need edges between them and are either:
        // * Edge start/end points (any edge touching the border "breaks" it into two sections except if it's in a corner)
        // * Corner points (unless an edge ends in a corner, then these are "terminal" points along each edge)

        // As we collect the nodes (basically, edge points on the border).
        // we keep them in a sorted order in the above clockwise manner.
        var nodes = new SortedSet<BorderNode>(new BorderNodeComparer());
        bool hadLeftTop = false;
        bool hadTopRight = false;
        bool hadBottomRight = false;
        bool hadLeftBottom = false;
        for (int i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (edge.Start.BorderLocation != Direction.None)
            {
                nodes.Add(new EdgeStartBorderNode(edge, i * 2));
                if (edge.Start.BorderLocation == Direction.LeftTop) hadLeftTop = true;
                else if (edge.Start.BorderLocation == Direction.TopRight) hadTopRight = true;
                else if (edge.Start.BorderLocation == Direction.BottomRight) hadBottomRight = true;
                else if (edge.Start.BorderLocation == Direction.LeftBottom) hadLeftBottom = true;
            }
            if (edge.End!.BorderLocation != Direction.None)
            {
                nodes.Add(new EdgeEndBorderNode(edge, i * 2 + 1));
                if (edge.End.BorderLocation == Direction.LeftTop) hadLeftTop = true;
                else if (edge.End.BorderLocation == Direction.TopRight) hadTopRight = true;
                else if (edge.End.BorderLocation == Direction.BottomRight) hadBottomRight = true;
                else if (edge.End.BorderLocation == Direction.LeftBottom) hadLeftBottom = true;
            }
        }
        // If none of the edges hit any of the corners, then we need to add those as generic non-edge nodes 
        if (!hadLeftTop)
            nodes.Add(new CornerBorderNode(new VoronoiPoint(minX, minY, Direction.LeftTop)));
        if (!hadTopRight) 
            nodes.Add(new CornerBorderNode(new VoronoiPoint(maxX, minY, Direction.TopRight)));
        if (!hadBottomRight)
            nodes.Add(new CornerBorderNode(new VoronoiPoint(maxX, maxY, Direction.BottomRight)));
        if (!hadLeftBottom)
            nodes.Add(new CornerBorderNode(new VoronoiPoint(minX, maxY, Direction.LeftBottom)));
        EdgeBorderNode? previousEdgeNode = null;
        if (nodes.Min is EdgeBorderNode febn)
            previousEdgeNode = febn;
        if (previousEdgeNode == null)
        {
            foreach (BorderNode node in nodes.Reverse())
            {
                if (node is EdgeBorderNode rebn)
                {
                    previousEdgeNode = rebn;
                    break;
                }
            }
        }
        VoronoiCell? defaultCell = null;
        if (previousEdgeNode == null)
        {
            // We have no edges within bounds
            if (cells.Count != 0)
            {
                // But we may have site(s), so it's possible a site is in the bounds
                // (two sites couldn't be or there would be an edge)
                defaultCell = cells.FirstOrDefault(c =>
                    c.Site.X.ApproxGreaterThanOrEqualTo(minX) &&
                    c.Site.X.ApproxLessThanOrEqualTo(maxX) &&
                    c.Site.Y.ApproxGreaterThanOrEqualTo(minY) &&
                    c.Site.Y.ApproxLessThanOrEqualTo(maxY)
                    );
            }
        }
        // Edge tracking for neighbour recording
        VoronoiEdge firstEdge = null!; // to "loop" last edge back to first
        VoronoiEdge? previousEdge = null; // to connect each new edge to previous edg
        BorderNode? node2 = null; // i.e. last node
        foreach (var node in nodes)
        {
            var node1 = node2;
            node2 = node;
            if (node1 == null) // i.e. node == nodes.Min
                continue; // we are looking at first node, we will start from Min and next one
            var site = previousEdgeNode != null ? previousEdgeNode is EdgeStartBorderNode ? previousEdgeNode.Edge.Right : previousEdgeNode.Edge.Left : defaultCell;
            if (node1.Point != node2.Point)
            {
                var newEdge = new VoronoiEdge(
                    node1.Point,
                    node2.Point, // we are building these clockwise, so by definition the left side is out of bounds
                    site
                );
                // Record the first created edge for the last edge to "loop" around
                if (previousEdge is null)
                    firstEdge = newEdge;
                edges.Add(newEdge);
                site?.CellEdges.Add(newEdge);
                previousEdge = newEdge;
            }
            // Passing an edge node means that the site changes as we are now on the other side of this edge
            // (this doesn't happen in non-edge corner, which keep the same site)
            if (node is EdgeBorderNode cebn)
                previousEdgeNode = cebn;
        }
        var finalSite = previousEdgeNode != null ? previousEdgeNode is EdgeStartBorderNode ? previousEdgeNode.Edge.Right : previousEdgeNode.Edge.Left : defaultCell;
        var finalEdge = new VoronoiEdge(
            nodes.Max?.Point ?? throw new ArgumentNullException(),
            nodes.Min?.Point ?? throw new ArgumentNullException(), // we are building these clockwise, so by definition the left side is out of bounds
            finalSite
        );
        edges.Add(finalEdge);
        finalSite?.CellEdges.Add(finalEdge);
        return edges;
    }


    private abstract class BorderNode
    {
        public abstract Direction BorderLocation { get; }

        public abstract VoronoiPoint Point { get; }

        public abstract double Angle { get; }

        public abstract int FallbackComparisonIndex { get; }

        public int CompareAngleTo(BorderNode node2, Direction pointBorderLocation)
        {
            // "Normal" Atan2 returns an angle between -π ≤ θ ≤ π as "seen" on the Cartesian plane,
            // that is, starting at the "right" of x axis and increasing counter-clockwise.
            // But we want the angle sortable (counter-)clockwise along each side.
            // So we cannot have the origin be "crossable" by the angle.

            //             0..-π or π
            //             ↓←←←←←←←←
            //             ↓       ↑  π/2..π
            //  -π/2..π/2  X       O  -π/2..-π
            //             ↑       ↓
            //             ↑←←←←←←←←
            //             0..π or -π

            // Now we need to decide how to compare them based on the side 

            double angle1 = Angle;
            double angle2 = node2.Angle;

            switch (pointBorderLocation)
            {
                case Direction.Left:
                    // Angles are -π/2..π/2
                    // We don't need to adjust to have it in the same directly-comparable range
                    // Smaller angle comes first
                    break;

                case Direction.Bottom:
                    // Angles are 0..-π or π
                    // We can swap π to -π
                    // Smaller angle comes first
                    if (angle1.ApproxGreaterThan(0)) angle1 -= 2 * Math.PI;
                    if (angle2.ApproxGreaterThan(0)) angle2 -= 2 * Math.PI;
                    break;

                case Direction.Right:
                    // Angles are π/2..π or -π/2..-π
                    // We can swap <0 to >0
                    // Angles are now π/2..π or 3/2π..π, i.e. π/2..3/2π
                    if (angle1.ApproxLessThan(0)) angle1 += 2 * Math.PI;
                    if (angle2.ApproxLessThan(0)) angle2 += 2 * Math.PI;
                    break;

                case Direction.Top:
                    // Angles are 0..π or -π
                    // We can swap -π to π 
                    // Smaller angle comes first
                    if (angle1.ApproxLessThan(0)) angle1 += 2 * Math.PI;
                    if (angle2.ApproxLessThan(0)) angle2 += 2 * Math.PI;
                    break;

                case Direction.BottomRight:
                case Direction.TopRight:
                case Direction.LeftBottom:
                case Direction.LeftTop:
                case Direction.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pointBorderLocation), pointBorderLocation, null);
            }

            // Smaller angle comes first
            return angle1.ApproxCompareTo(angle2);
        }


#if DEBUG
        public override string ToString()
        {
            return Point + " @ " + BorderLocation;
        }

        public string ToString(string format)
        {
            return Point.ToString(format) + " @ " + BorderLocation;
        }
#endif
    }

    private abstract class EdgeBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) : BorderNode
    {
        public VoronoiEdge Edge { get; } = edge;

        public override int FallbackComparisonIndex { get; } = fallbackComparisonIndex;
    }

    private class EdgeStartBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
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

    private class EdgeEndBorderNode(VoronoiEdge edge, int fallbackComparisonIndex) :
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

    private class CornerBorderNode(VoronoiPoint point) : BorderNode
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

    private class BorderNodeComparer : IComparer<BorderNode>
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
}