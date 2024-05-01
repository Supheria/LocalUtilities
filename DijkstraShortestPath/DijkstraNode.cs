using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LocalUtilities.DijkstraShortestPath;

internal class DijkstraNode(Coordinate node, int index)
{
    public bool Used { get; set; } = false;
    public List<Coordinate> Nodes { get; } = [];

    public Coordinate Node { get; set; } = node;

    public int Index { get; set; } = index;

    public double Weight { get; set; } = double.MaxValue;
}
