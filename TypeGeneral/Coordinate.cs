using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Math;

namespace LocalUtilities.TypeGeneral;

public enum CoordinateType
{
    X,
    Y,
}

public class Coordinate(double x, double y)
{
    public double X { get; } = x;

    public double Y { get; } = y;

    public Coordinate() : this(0, 0)
    {

    }

    public static implicit operator PointF(Coordinate coordinate)
    {
        return new((float)coordinate.X, (float)coordinate.Y);
    }

    public static bool operator ==(Coordinate? coordinate, object? obj)
    {
        if (coordinate is null)
        {
            if (obj is null)
                return true;
            else
                return false;
        }
        if (obj is not Coordinate other)
            return false;
        return coordinate.X.ApproxEqualTo(other.X) && coordinate.Y.ApproxEqualTo(other.Y);
    }

    public static bool operator !=(Coordinate? coordinate, object? obj)
    {
        return !(coordinate == obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override string ToString()
    {
        return (X, Y).ToArrayString();
    }

    public string ToIntString()
    {
        return ((int)X, (int)Y).ToArrayString();
    }

    public double Parse(CoordinateType type)
    {
        return type switch
        {
            CoordinateType.X => X,
            CoordinateType.Y => Y,
            _ => throw new InvalidOperationException()
        };
    }

    public static double Parse(Coordinate coordinate, CoordinateType type)
    {
        return type switch
        {
            CoordinateType.X => coordinate.X,
            CoordinateType.Y => coordinate.Y,
            _ => throw new InvalidOperationException()
        };
    }
    public static Coordinate Parse(string str)
    {
        var list = str.ToArray();
        if (list.Length is 2)
            return new(int.Parse(list[0]), int.Parse(list[1]));
        throw TypeConvertException.CannotConvertStringTo<Coordinate>();
    }
}
