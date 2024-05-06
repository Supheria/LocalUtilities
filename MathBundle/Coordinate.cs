namespace LocalUtilities.MathBundle;

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

    public static bool operator ==(Coordinate? c1, object? c2)
    {
        if (c1 is null)
        {
            if (c2 is null)
                return true;
            else
                return false;
        }
        if (c2 is not Coordinate other)
            return false;
        return c1.X.ApproxEqual(other.X) && c1.Y.ApproxEqual(other.Y);
    }

    public static bool operator !=(Coordinate? c1, object? c2)
    {
        return !(c1 == c2);
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
