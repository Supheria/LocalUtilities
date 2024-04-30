using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.GdiUtilities;

public class Coordinate(double x, double y, int column, int row)
{
    public double X { get; } = x;

    public double Y { get; } = y;

    public int Column { get; } = column;

    public int Row { get; } = row;

    public Coordinate() : this(0, 0, 0, 0)
    {

    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Column, Row);
    }
}
