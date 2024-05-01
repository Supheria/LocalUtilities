using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities;

public class Coordinate(double x, double y)
{
    public double X { get; } = x;

    public double Y { get; } = y;

    public Coordinate() : this(0, 0)
    {

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
            return X == c.X && Y == c.Y;
        return false;
    }
}
