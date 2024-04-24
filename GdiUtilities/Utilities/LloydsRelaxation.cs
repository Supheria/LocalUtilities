using LocalUtilities.GdiUtilities.VoronoiDiagram.Structure;

namespace LocalUtilities.GdiUtilities.Utilities;

internal class LloydsRelaxation
{
    public void Relax(List<VoronoiSite> sites, double minX, double minY, double maxX, double maxY, float strength)
    {
        bool fullStrength = Math.Abs(strength - 1.0f) < float.Epsilon;

        foreach (VoronoiSite site in sites)
        {
            VoronoiPoint centroid = site.GetCentroid();

            if (fullStrength)
            {
                site.Relocate(centroid.X, centroid.Y);
            }
            else
            {
                double newX = site.X + (centroid.X - site.X) * strength;
                double newY = site.Y + (centroid.Y - site.Y) * strength;

                site.Relocate(newX, newY);
            }
        }
    }
}