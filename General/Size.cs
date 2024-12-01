namespace LocalUtilities.General;

public class Size
{
    public int Width { get; private set; }

    public int Height { get; private set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public Size()
    {
        Width = 0;
        Height = 0;
    }
}
