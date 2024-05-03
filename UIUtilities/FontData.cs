using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.UIUtilities;

public class FontData(string familyName, float scaleFactorToHeight, FontStyle style, GraphicsUnit unit)
{
    public string FamilyName { get; set; } = familyName;

    public float ScaleFactorToHeight { get; set; } = scaleFactorToHeight;

    public FontStyle Style { get; set; } = style;

    public GraphicsUnit Unit { get; set; } = unit;

    public FontData() : this("黑体", 0.03f, FontStyle.Regular, GraphicsUnit.Pixel) 
    {

    }
}
