namespace LocalUtilities.GraphUtilities;

public class Edge(Coordinate starter, Coordinate ender)
{
    public Coordinate Starter { get; } = starter;

    public Coordinate Ender { get; } = ender;

    public override int GetHashCode()
    {
        return HashCode.Combine(Starter.GetHashCode(), Ender.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj is Edge e)
            return Starter == e.Starter && Ender == e.Ender;
        return false;
    }
}
