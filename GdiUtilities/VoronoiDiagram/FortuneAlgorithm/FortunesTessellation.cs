using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;
using VoronoiDiagram;

namespace LocalUtilities.GdiUtilities.VoronoiDiagram.FortuneAlgorithm;

internal class FortunesTessellation
{
    public List<VoronoiEdge> Run(List<VoronoiSite> sites, double minX, double minY, double maxX, double maxY)
    {
        var eventQueue = new MinHeap<IFortuneEvent>(5 * sites.Count);

        foreach (VoronoiSite site in sites)
        {
            ArgumentNullException.ThrowIfNull(site);
            eventQueue.Insert(new FortuneSiteEvent(site));
        }


        //init tree
        BeachLine beachLine = new BeachLine();
        LinkedList<VoronoiEdge> edges = new LinkedList<VoronoiEdge>();
        HashSet<FortuneCircleEvent> deleted = new HashSet<FortuneCircleEvent>();

        //init edge list
        while (eventQueue.Count != 0)
        {
            IFortuneEvent fEvent = eventQueue.Pop();
            if (fEvent is FortuneSiteEvent)
                beachLine.AddBeachSection((FortuneSiteEvent)fEvent, eventQueue, deleted, edges);
            else
            {
                if (deleted.Contains((FortuneCircleEvent)fEvent))
                {
                    deleted.Remove((FortuneCircleEvent)fEvent);
                }
                else
                {
                    beachLine.RemoveBeachSection((FortuneCircleEvent)fEvent, eventQueue, deleted, edges);
                }
            }
        }

        return edges.ToList();
        // TODO: Build the list directly
    }
}