using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace LocalUtilities.GdiUtilities.VoronoiDiagram.FortuneAlgorithm;

internal class FortuneSiteEvent : IFortuneEvent
{
    public double X => Site.X;
    public double Y => Site.Y;
    internal VoronoiSite Site { get; }

    internal FortuneSiteEvent(VoronoiSite site)
    {
        Site = site;
    }

    public int CompareTo(IFortuneEvent? other)
    {
        if (other is null)
        {
            if (this is null)
                return 0;
            else
                throw new ArgumentNullException(nameof(other));
        }
        int c = Y.CompareTo(other.Y);
        return c == 0 ? X.CompareTo(other.X) : c;
    }

}