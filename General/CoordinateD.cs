namespace LocalUtilities.General;

public class CoordinateD
{
    public double X { get; private set; }

    public double Y { get; private set; }

    public CoordinateD(double x, double y)
    {
        X = x;
        Y = y;
    }

    public CoordinateD()
    {
        X = 0;
        Y = 0;
    }
}
