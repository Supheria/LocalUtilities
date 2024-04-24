namespace LocalUtilities.GdiUtilities.VoronoiDiagram.FortuneAlgorithm;

interface IFortuneEvent : IComparable<IFortuneEvent>
{
    double X { get; }
    double Y { get; }
}
