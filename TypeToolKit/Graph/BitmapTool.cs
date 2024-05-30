using LocalUtilities.TypeGeneral;
using System.Drawing.Drawing2D;

namespace LocalUtilities.TypeToolKit.Graph;

public static class BitmapTool
{
    public static Bitmap CopyToNewSize(this Bitmap source, int width, int height, InterpolationMode mode)
    {
        var target = new Bitmap(width, height);
        var g = Graphics.FromImage(target);
        g.Clear(Color.White);
        g.InterpolationMode = mode;
        g.DrawImage(source, 0, 0, width, height);
        g.Flush(); g.Dispose();
        source.Dispose();
        return target;
    }

    public static void TemplateDrawIntoRect(this Bitmap template, Bitmap target, Rectangle toRect, bool ignoreTransparent)
    {
        var dWidth = toRect.Width - template.Width;
        var dHeight = toRect.Height - template.Height;
        var startX = dWidth < 0 ? throw GraphicException.SizeOutRange(template.Size, toRect) : toRect.Left + dWidth / 2;
        var startY = dHeight < 0 ? throw GraphicException.SizeOutRange(template.Size, toRect) : toRect.Top + dHeight / 2;
        var pTemplate = new PointBitmap(template);
        var pTarget = new PointBitmap(target);
        pTemplate.LockBits();
        pTarget.LockBits();
        for (int x = 0; x < template.Width; x++)
        {
            for (int y = 0; y < template.Height; y++)
            {
                var pixel = pTemplate.GetPixel(x, y);
                if (!ignoreTransparent || pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                    pTarget.SetPixel(x + startX, y + startY, pixel);
            }
        }
        pTemplate.UnlockBits();
        pTarget.UnlockBits();
    }

    public static void TemplateDrawPartsOn(this Bitmap template, Bitmap source, List<Rectangle> drawRects, bool ignoreTransparent)
    {
        if (source.Size != template.Size)
            throw GraphicException.SizeMismatch();
        PointBitmap pTemplate = new(template);
        PointBitmap pTarget = new(source);
        pTemplate.LockBits();
        pTarget.LockBits();
        foreach (var rect in drawRects)
        {
            for (var x = rect.X; x < rect.Right; x++)
            {
                for (var y = rect.Y; y < rect.Bottom; y++)
                {
                    var pixel = pTemplate.GetPixel(x, y);
                    if (!ignoreTransparent || pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                    {
                        pTarget.SetPixel(x, y, pixel);
                    }
                }
            }
        }
        pTemplate.UnlockBits();
        pTarget.UnlockBits();
    }
}
