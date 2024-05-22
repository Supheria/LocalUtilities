using LocalUtilities.TypeGeneral;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LocalUtilities.TypeToolKit.Graph;

public static class BitmapTool
{
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

    public static void TemplateDrawPartOn(this Bitmap template, Bitmap source, Rectangle drawPart, bool ignoreTransparent)
    {
        if (source.Size != template.Size)
            throw GraphicException.SizeMismatch();
        PointBitmap pTemplate = new(template);
        PointBitmap pTarget = new(source);
        pTemplate.LockBits();
        pTarget.LockBits();
        var right = drawPart.Right;
        var bottom = drawPart.Bottom;
        for (var x = drawPart.X; x < right; x++)
        {
            for (var y = drawPart.Y; y < bottom; y++)
            {
                var pixel = pTemplate.GetPixel(x, y);
                if (!ignoreTransparent || pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                {
                    pTarget.SetPixel(x, y, pixel);
                }
            }
        }
        pTemplate.UnlockBits();
        pTarget.UnlockBits();
    }
}
