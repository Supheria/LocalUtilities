using System.Runtime.CompilerServices;

namespace LocalUtilities.VoronoiDiagram;

public static class MathTool
{
    public static double EvalParabola(double focusX, double focusY, double directrix, double x)
    {
        return .5 * ((x - focusX) * (x - focusX) / (focusY - directrix) + focusY + directrix);
    }

    //gives the intersect point such that parabola 1 will be on top of parabola 2 slightly before the intersect
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double IntersectParabolaX(double focus1X, double focus1Y, double focus2X, double focus2Y,
        double directrix)
    {
        //admittedly this is pure voodoo.
        //there is attached documentation for this function
        return focus1Y.ApproxEqual(focus2Y)
            ? (focus1X + focus2X) / 2
            : (focus1X * (directrix - focus2Y) + focus2X * (focus1Y - directrix) +
               Math.Sqrt((directrix - focus1Y) * (directrix - focus2Y) *
                         ((focus1X - focus2X) * (focus1X - focus2X) +
                          (focus1Y - focus2Y) * (focus1Y - focus2Y))
               )
              ) / (focus1Y - focus2Y);
    }

    //private const double epsilon = double.Epsilon * 1E100;
    // The above is the original epsilon used for the algorithm.
    // This doesn't actually work since precision is lost after 15 digits for double.
    // Even with multiplier it's still like 4.94E-224, which is ridiculously beyond the loss of precision threshold.
    // For example, beach line edge intercept stuff would only capture precision to something like:
    // 999.99999999999989 vs 1000
    // In fact, anything less than e^-12 will immediatelly fail some coordinate comparisons because of all the compounding precision losses.
    // Of course, numbers too large will start failing again since we can't exactly compare significant digits (cheaply).
    static double Epsilon { get; } = 1E-12;


    public static bool ApproxEqual(this double value1, double value2)
    {
        return value1 - value2 < Epsilon &&
               value2 - value1 < Epsilon;
    }

    public static bool ApproxGreaterThan(this double value1, double value2)
    {
        return value1 > value2 + Epsilon;
    }

    public static bool ApproxGreaterThanOrEqualTo(this double value1, double value2)
    {
        return value1 > value2 - Epsilon;
    }

    public static bool ApproxLessThan(this double value1, double value2)
    {
        return value1 < value2 - Epsilon;
    }

    public static bool ApproxLessThanOrEqualTo(this double value1, double value2)
    {
        return value1 < value2 + Epsilon;
    }

    public static int ApproxCompareTo(this double value1, double value2)
    {
        if (value1.ApproxGreaterThan(value2))
            return 1;

        if (value1.ApproxLessThan(value2))
            return -1;

        return 0;
    }
}