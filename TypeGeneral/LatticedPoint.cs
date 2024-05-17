using LocalUtilities.TypeToolKit.Convert;
using System.Diagnostics.CodeAnalysis;

namespace LocalUtilities.TypeGeneral;

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

    public static LatticedPoint Parse(string str)
    {
        var list = str.ToArray();
        if (list.Length is 2)
            return new(int.Parse(list[0]), int.Parse(list[1]));
        throw TypeConvertException.CannotConvertStringTo<LatticedPoint>();
    }
}
