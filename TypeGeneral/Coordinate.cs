using LocalUtilities.TypeGeneral.Convert;
using LocalUtilities.TypeToolKit.Mathematic;

namespace LocalUtilities.TypeGeneral;

public class Coordinate(int x, int y)
{
    public int X { get; } = x;

    public int Y { get; } = y;

    public Coordinate() : this(0, 0)
    {

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
        return coordinate.X == other.X && coordinate.Y == other.Y;
    }

    public static bool operator !=(Coordinate? coordinate, object? obj)
    {
        return !(coordinate == obj);
    }

    public static Coordinate operator /(Coordinate coordinate, double factor)
    {
        return new((coordinate.X / factor).ToInt(), (coordinate.Y / factor).ToInt());
    }

    public static Coordinate operator -(Coordinate left, Coordinate right)
    {
        return new(left.X - right.X, left.Y - right.Y);
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

    public static Coordinate Parse(string str)
    {
        var list = str.ToArray();
        if (list.Length is 2)
            return new(int.Parse(list[0]), int.Parse(list[1]));
        throw TypeConvertException.CannotConvertStringTo<Coordinate>();
    }

    public static implicit operator PointF(Coordinate coordinate)
    {
        return new(coordinate.X, coordinate.Y);
    }
}
