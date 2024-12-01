namespace LocalUtilities.General;

public class Rectangle
{
    public int Left { get; private set; }

    public int Top { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Right => Left + Width;

    public int Bottom => Top + Height;

    public Size Size => new(Width, Height);

    public Rectangle(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    public Rectangle(Coordinate leftTop, Size size)
    {
        Left = leftTop.X;
        Top = leftTop.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public Rectangle()
    {
        Left = 0;
        Top = 0;
        Width = 0;
        Height = 0;
    }

    public bool Contains(int x, int y)
    {
        var vertexes = new List<Coordinate>()
        {
            new(Left, Top),
            new(Right, Top),
            new(Left, Bottom),
            new(Right, Bottom)
        };
        return GeometryTool.PointInPolygon(vertexes, x, y);
    }
}
