using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit.Mathematic;

public static class GeometryTool
{
    public static Size ScaleSizeOnRatio(this Size fromSize, Size toSize)
    {
        var toWidth = toSize.Width;
        var toHeight = toSize.Height;
        var toRatio = toSize.Width / (double)toSize.Height;
        var sourceRatio = fromSize.Width / (double)fromSize.Height;
        if (sourceRatio > toRatio)
        {
            toWidth = toSize.Width;
            toHeight = (int)(toWidth / sourceRatio);
        }
        else if (sourceRatio < toRatio)
        {
            toHeight = toSize.Height;
            toWidth = (int)(toHeight * sourceRatio);
        }
        return new(toWidth, toHeight);
    }
}
