using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities;

[Flags]
public enum Direction
{
    None,
    Left,
    Top,
    Right,
    Bottom,
    LeftTop,
    TopRight,
    LeftBottom,
    BottomRight,
}
