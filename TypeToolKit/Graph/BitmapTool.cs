using LocalUtilities.TypeGeneral;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LocalUtilities.TypeToolKit.Graph;

public static class BitmapTool
{
    public static void DrawTemplateIntoRect(this Image image, Bitmap template, Rectangle toRect, bool ignoreTransparent)
    {
        DrawTemplateIntoRect((Bitmap)image, template, toRect, ignoreTransparent);
    }

    public static void DrawTemplateIntoRect(this Bitmap source, Bitmap template, Rectangle toRect, bool ignoreTransparent)
    {
        var dWidth = toRect.Width - template.Width;
        var dHeight = toRect.Height - template.Height;
        var startX = dWidth > 0 ? toRect.Left + dWidth / 2 : toRect.Left;
        var startY = dHeight > 0 ? toRect.Top + dHeight / 2 : toRect.Top;
        var pSource = new PointBitmap(source);
        var pTemplate = new PointBitmap(template);
        pSource.LockBits();
        pTemplate.LockBits();
        for (int x = dWidth < 0 ? Math.Abs(dWidth) / 2 : 0; x < Math.Min(template.Width, toRect.Width); x++)
        {
            for (int y = dHeight < 0 ? Math.Abs(dHeight) / 2 : 0; y < Math.Min(template.Height, toRect.Height); y++)
            {
                var pixel = pTemplate.GetPixel(x, y);
                if (!ignoreTransparent || pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                    pSource.SetPixel(x + startX, y + startY, pixel);
            }
        }
        pSource.UnlockBits();
        pTemplate.UnlockBits();
    }

    public static Size ScaleToSizeOnRatio(this Bitmap source, Size toSize)
    {
        var toWidth = toSize.Width;
        var toHeight = toSize.Height;
        var toRatio = toSize.Width / (double)toSize.Height;
        var sourceRatio = source.Width / (double)source.Height;
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

    public static Bitmap CopyToNewSize(this Bitmap source, Size toSize)
    {
        var target = new Bitmap(toSize.Width, toSize.Height);
        var g = Graphics.FromImage(target);
        g.Clear(Color.White);
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.DrawImage(source, 0, 0, toSize.Width, toSize.Height);
        g.Flush(); g.Dispose();
        return target;
    }
}
