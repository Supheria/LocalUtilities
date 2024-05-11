using LocalUtilities.StringUtilities;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.MathBundle;

public class LatticedPoint
{
    public int Col { get; set; }

    public int Row { get; set; }

    public LatticedPoint()
    {
        Col = 0;
        Row = 0;
    }

    public LatticedPoint(int col, int row)
    {
        Col = col;
        Row = row;
    }

    public static bool operator ==(LatticedPoint latticedPoint, object? obj)
    {
        if (obj is not LatticedPoint other)
            return false;
        return latticedPoint.Col == other.Col && latticedPoint.Row == other.Row;
    }

    public static bool operator !=(LatticedPoint latticedPoint, object? obj)
    {
        return !(latticedPoint == obj);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return this == obj;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Col, Row);
    }

    public override string ToString()
    {
        return (Col, Row).ToArrayString();
    }
}
