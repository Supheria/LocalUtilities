namespace LocalUtilities.MathBundle;

[Flags]
public enum Direction
{
    None = 0x0,
    Left = 0x1,
    Top = 0x2,
    Right = 0x4,
    Bottom = 0x8,
    LeftTop = Left | Top,
    TopRight = Top | Right,
    LeftBottom = Left | Bottom,
    BottomRight = Bottom | Right,
}

public static class DirectionBundle
{
    /// <summary>
    /// start from LeftTop, walk along counter-clockwisely the value of direction sets to bigger 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static int CompareToCounterClockwisely(this Direction direction, Direction other)
    {
        return direction.CounterClockwiseOrder().CompareTo(other.CounterClockwiseOrder());
    }

    private static int CounterClockwiseOrder(this Direction direction)
    {
        return direction switch
        {
            Direction.LeftTop => 0,
            Direction.Left => 1,
            Direction.LeftBottom => 2,
            Direction.Bottom => 3,
            Direction.BottomRight => 4,
            Direction.Right => 5,
            Direction.TopRight => 6,
            Direction.Top => 7,
            _ => throw new InvalidOperationException(),
        };
    }
}