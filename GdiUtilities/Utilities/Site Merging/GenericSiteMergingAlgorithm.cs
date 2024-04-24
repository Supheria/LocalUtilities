﻿using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace VoronoiDiagram;

public class GenericSiteMergingAlgorithm
{
    public void MergeSites(List<VoronoiSite> sites, List<VoronoiEdge> edges, VoronoiSiteMergeQuery mergeQuery)
    {
        if (sites.Count < 2)
            return;

        while (true)
        {
            bool anyMerged = false;

            for (int s = 0; s < sites.Count; s++)
            {
                VoronoiSite site = sites[s];

                bool selfMerged = false;

                // Try to merge with any of our neighbours

                for (int n = 0; n < site.Neighbours.Count; n++)
                {
                    VoronoiSite neighbour = site.Neighbours[n];
                    // Note that if we merge previous site, we may get new neighbours - it's okay to try to merge them too

                    VoronoiSiteMergeDecision mergeDecision = mergeQuery.Invoke(site, neighbour);

                    switch (mergeDecision)
                    {
                        case VoronoiSiteMergeDecision.DontMerge:
                            // TODO: RECORD THAT WE TRIED THIS
                            // TODO: OPTIONAL? E.G. MERGE RULES CHANGE BASED ON OTHER SITES OR WHATEVER 
                            break;

                        case VoronoiSiteMergeDecision.MergeIntoSite1:
                            // Merge the neighbour into ourselves - we survive, neighbour is removed
                            int removalIndex = PerformMerge(site, neighbour, sites, edges);

                            anyMerged = true;

                            // If we removed a site before ourselves in the list, we need to shift back iteration 
                            if (removalIndex < s)
                                s--;
                            break;

                        case VoronoiSiteMergeDecision.MergeIntoSite2:
                            // Merge ourselves into the neighbour  - neighbour survives, we are removed
                            PerformMerge(neighbour, site, sites, edges);

                            selfMerged = true;
                            anyMerged = true;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (selfMerged)
                        break; // the site is no longer usable, further neighbours must merge into the other/new site 
                }
            }

            if (sites.Count == 1)
                break; // we have merged everything into a single site

            if (!anyMerged) // keep going until no more merges happen
                break;
        }

#if DEBUG
        if (_tempEdges.Count != 0) throw new NotImplementedException("Didn't clean up " + nameof(_tempEdges));
#endif
    }


    private int PerformMerge(VoronoiSite target, VoronoiSite source, List<VoronoiSite> sites, List<VoronoiEdge> edges)
    {
        // Pre-computed values are no longer valid for the site since we will change its geometry
        target.InvalidateComputedValues();

        // Remove common edges (may be more than 1 if we have already merged nearby previously)

        List<VoronoiEdge> commonEdges = GetCommonEdges(target, source);

        foreach (VoronoiEdge commonEdge in commonEdges)
        {
            edges.Remove(commonEdge);
            target.CellEdges.Remove(commonEdge);
            source.CellEdges.Remove(commonEdge); // we will need to process source cells list later
        }

        // Unlink sites from each other

        source.Neighbours.Remove(target);
        target.Neighbours.Remove(source);

        // Update links for other old site's neighbours

        foreach (VoronoiSite neighbour in source.Neighbours)
        {
            // Old site is gone and we are the neighbour now
            neighbour.Neighbours.Remove(source);

            // If we aren't already connected to the old site's neighbour...
            if (!target.Neighbours.Contains(neighbour))
            {
                target.Neighbours.Add(neighbour); // new neighbour (prevously old site's neighbour)
                neighbour.Neighbours.Add(target); // backlink
            }
        }

        // "Transfer" new edges from the old site to the new one

        foreach (VoronoiEdge edge in source.CellEdges)
            if (!target.CellEdges.Contains(edge))
                target.CellEdges.Add(edge);

        // Invalidate source site completely - it's no longer part of the plane
        source.Invalidated();

        // Remove site from the main list - will never iterate it again
        int removalIndex = sites.IndexOf(source);
        sites.RemoveAt(removalIndex);

        // Clean-up
        _tempEdges.Clear(); // don't cling to any references

        return removalIndex;
    }

    private List<VoronoiEdge> GetCommonEdges(VoronoiSite site1, VoronoiSite site2)
    {
#if DEBUG
        if (_tempEdges.Count != 0) throw new NotImplementedException("Didn't clean up " + nameof(_tempEdges));
#endif

        foreach (VoronoiEdge edge1 in site1.CellEdges)
        {
            foreach (VoronoiEdge edge2 in site2.CellEdges)
            {
                if (edge1 == edge2)
                    _tempEdges.Add(edge1);
            }
        }

        return _tempEdges;
    }

    private readonly List<VoronoiEdge> _tempEdges = new List<VoronoiEdge>(); // cached for GC
}