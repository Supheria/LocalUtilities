﻿using LocalUtilities.TypeToolKit.Mathematic;

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
        return new((coordinate.X / factor).ToRoundInt(), (coordinate.Y / factor).ToRoundInt());
    }

    public static Coordinate operator +(Coordinate left, Coordinate right)
    {
        return new(left.X + right.X, left.Y + right.Y);
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
        var array = str.ToArray();
        if (array.Length is not 2 ||
            !int.TryParse(array[0], out var x) ||
            !int.TryParse(array[1], out var y))
            return new();
        return new(x, y);
    }

    public static implicit operator PointF(Coordinate? coordinate)
    {
        if (coordinate is null)
            return new();
        return new(coordinate.X, coordinate.Y);
    }

    public static implicit operator Point(Coordinate? coordinate)
    {
        if (coordinate is null)
            return new();
        return new(coordinate.X, coordinate.Y);
    }
}
