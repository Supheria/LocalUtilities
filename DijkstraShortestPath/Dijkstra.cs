using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace LocalUtilities.DijkstraShortestPath;

public class Dijkstra
{
    static double[,] Graph { get; set; } = new double[0, 0];

    /// <summary>
    /// 所有的边
    /// </summary>
    static List<DijkstraEdge> Edges { get; set; } = [];

    /// <summary>
    /// 所有的点
    /// </summary>
    static List<Coordinate> Vertexes { get; set; } = [];

    static List<DijkstraNode> NodeItems { get; set; } = [];

    public static void Initialize(List<Edge> edges, List<Coordinate> nodes)
    {
        Edges = edges.Select(e => new DijkstraEdge(e)).ToList();
        Vertexes = nodes;
        NodeItems = [];
        Graph = new double[Vertexes.Count, Vertexes.Count];
        foreach (var row in Enumerable.Range(0, Vertexes.Count))
        {
            var rowNode = Vertexes[row];
            foreach (var colnum in Enumerable.Range(0, Vertexes.Count))
            {
                if (row == colnum)
                {
                    Graph[row, colnum] = 0;
                    continue;
                }
                var edge = Edges.FirstOrDefault(x => x.Edge.Starter == rowNode && x.Edge.Ender == Vertexes[colnum]);
                Graph[row, colnum] = edge == null ? double.MaxValue : edge.Weight;
            }
            NodeItems.Add(new(Vertexes[row], row));
        }
    }

    public static List<Edge> GetRoute(Coordinate startVertex, Coordinate endVertex)
    {
        if (Vertexes.FirstOrDefault(c => c == startVertex) is null ||
            Vertexes.FirstOrDefault(c => c == endVertex) is null)
            throw new ArgumentNullException();
        var path = new List<Edge>();
        if (NodeItems.Count is 0)
            return path;
        NodeItems.First(n => n.Node == startVertex).Used = true;
        NodeItems.ForEach(x =>
        {
            x.Weight = GetRowArray(Graph, Vertexes.IndexOf(startVertex))[x.Index];
            x.Nodes.Add(startVertex);
        });
        while (NodeItems.Any(x => !x.Used))
        {
            var item = GetUnUsedAndMinNodeItem();
            if (item == null)
                break;
            item.Used = true;
            var tempRow = GetRowArray(Graph, item.Index);
            foreach (var nodeItem in NodeItems)
            {
                if (nodeItem.Weight > tempRow[nodeItem.Index] + item.Weight)
                {
                    nodeItem.Weight = tempRow[nodeItem.Index] + item.Weight;
                    nodeItem.Nodes.Clear();
                    nodeItem.Nodes.AddRange(item.Nodes);
                    nodeItem.Nodes.Add(item.Node);
                }
            }
        }
        var desNodeitem = NodeItems.First(x => x.Node == endVertex);
        if (desNodeitem.Used && desNodeitem.Weight < double.MaxValue)
        {
            foreach (var index in Enumerable.Range(0, desNodeitem.Nodes.Count - 1))
            {
                var e = Edges.FirstOrDefault(e => e.Edge.Starter == desNodeitem.Nodes[index] && e.Edge.Ender == desNodeitem.Nodes[index + 1]);
                if (e is not null)
                    path.Add(e.Edge);
            }
            var edge = Edges.FirstOrDefault(x => x.Edge.Starter == desNodeitem.Nodes.Last() && x.Edge.Ender == endVertex);
            if(edge is not null)
                path.Add(edge.Edge);
        }
        NodeItems.ForEach(x =>
        {
            x.Used = false;
            x.Nodes.Clear();
        });
        return path;
    }

    private static DijkstraNode? GetUnUsedAndMinNodeItem()
    {
        return NodeItems.Where(x => !x.Used && x.Weight != double.MaxValue).OrderBy(x => x.Weight).FirstOrDefault();
    }

    private static double[] GetRowArray(double[,] source, int row)
    {
        double[] result = new double[source.GetLength(1)];
        foreach (var index in Enumerable.Range(0, result.Length))
        {
            result[index] = source[row, index];
        }
        return result;
    }
}
