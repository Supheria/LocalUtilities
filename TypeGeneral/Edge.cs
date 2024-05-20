using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeGeneral;

public class Edge(Coordinate starter, Coordinate ender)
{
    public Coordinate Starter { get; } = starter;

    public Coordinate Ender { get; } = ender;

    public double Length
    {
        get
        {
            _length ??= Math.Sqrt(Math.Pow(Ender.X - Starter.X, 2) + Math.Pow(Ender.Y - Starter.Y, 2));
            return _length.Value;
        }
    }
    double? _length = null;

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

    public List<string> ToStringArray()
    {
        return [Starter.ToString(), Ender.ToString()];
    }

    public static Edge Parse(List<string> array)
    {
        return array.Count is 2
            ? new(Coordinate.Parse(array[0]), Coordinate.Parse(array[1]))
            : throw TypeConvertException.CannotConvertStringArrayTo<Edge>();
    }
}
