﻿namespace LocalUtilities.TypeGeneral;

public class FontData
{
    public string FamilyName { get; set; } = "黑体";

    public float Size { get; set; } = 15f;

    public FontStyle Style { get; set; } = FontStyle.Regular;

    public GraphicsUnit Unit { get; set; } = GraphicsUnit.Pixel;

    public static implicit operator Font(FontData data)
    {
        return new(data.FamilyName, data.Size, data.Style, data.Unit);
    }
}
