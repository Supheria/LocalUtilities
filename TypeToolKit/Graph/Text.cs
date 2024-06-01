namespace LocalUtilities.TypeToolKit.Graph;

public static class Text
{
    public static void DrawUniformString(this Graphics g, Rectangle rect, string text, float charWidth, Color color, Font font)
    {
        var format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        var blankWidth = (rect.Width - charWidth * text.Length) / (float)(text.Length + 1);
        float left = rect.Left;
        var index = 0;
        using var brush = new SolidBrush(color);
        for (var i = 1; i < text.Length * 2 + 1; i++)
        {
            if (i % 2 is 0)
            {
                left += charWidth;
                continue;
            }
            var ch = text[index].ToString();
            left += blankWidth;
            var chRect = new RectangleF(left, rect.Top, charWidth, rect.Height);
            g.DrawString(ch, font, brush, chRect, format);
            index++;
        }
    }
}
