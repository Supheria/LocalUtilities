using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.DijkstraShortestPath;

internal class DijkstraEdge
{
    public Edge Edge { get; }

    public double Weight { get; }

    public DijkstraEdge(Edge edge)
    {
        Edge = edge;
        var (x1, y1) = (Edge.Starter.X, Edge.Starter.Y);
        var (x2, y2) = (Edge.Ender.X, Edge.Ender.Y);
        var x = Math.Pow(x1 - x2, 2);
        var y = Math.Pow(y1 - y2, 2);
        Weight = Math.Pow(x + y, 0.5);
    }
}
