using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities;

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
