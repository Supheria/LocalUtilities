using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities;

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
