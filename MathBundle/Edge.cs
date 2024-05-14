using LocalUtilities.TypeBundle;
using System.Collections.Generic;

namespace LocalUtilities.MathBundle;

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

    public List<string> ToStringArray(bool useInt)
    {
        return useInt ? [Starter.ToIntString(), Ender.ToIntString()] : [Starter.ToString(), Ender.ToString()];
    }

    public static Edge? ParseStringArray(List<string> array)
    {
        return array.Count is 2 ? new(array[0].ToCoordinate(new()), array[1].ToCoordinate(new())) : null;
    }
}
