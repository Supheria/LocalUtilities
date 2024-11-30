using LocalUtilities.TypeToolKit.Mathematic;

namespace LocalUtilities;

public class Coordinate(int x, int y)
{
    public int X { get; private set; } = x;

    public int Y { get; private set; } = y;

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
        return new((coordinate.X / factor).ToRoundInt(), (coordinate.Y / factor).ToRoundInt());
    }

    public static Coordinate operator +(Coordinate left, (int X, int Y) right)
    {
        return new(left.X + right.X, left.Y + right.Y);
    }

    public static Coordinate operator -(Coordinate left, (int X, int Y) right)
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
}
