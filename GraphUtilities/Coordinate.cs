using LocalUtilities.MathBundle;

namespace LocalUtilities.GraphUtilities;

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

    public static bool operator ==(Coordinate c1, Coordinate c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator !=(Coordinate c1, Coordinate c2)
    {
        return !(c1 == c2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj is Coordinate c)
            return X.ApproxEqual(c.X) && Y.ApproxEqual(c.Y);
        return false;
    }
}
