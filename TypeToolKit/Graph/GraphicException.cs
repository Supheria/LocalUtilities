using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.Graph;

public class GraphicException(string message) : Exception(message)
{
    public static GraphicException SizeOutRange(Size size, Rectangle rect)
    {
        return new($"size fo {size} is out of range of {rect}");
    }

    public static GraphicException SizeMismatch()
    {
        return new($"sizes are mismatch");
    }
}
