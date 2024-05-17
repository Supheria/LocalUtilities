using LocalUtilities.TypeGeneral;

namespace LocalUtilities.TypeToolKit.Graph;

public static class BitmapTool
{
    public static void DrawBitmapPartOn(this Bitmap source, Bitmap target, Rectangle drawOnRect, bool ignoreTransparent)
    {
        PointBitmap pSource = new(source);
        PointBitmap pTarget = new(target);
        pSource.LockBits();
        pTarget.LockBits();
        var right = drawOnRect.Right;
        var bottom = drawOnRect.Bottom;
        for (var x = drawOnRect.X; x < right; x++)
        {
            for (var y = drawOnRect.Y; y < bottom; y++)
            {
                var pixel = pSource.GetPixel(x, y);
                if (!ignoreTransparent || pixel.A != 0 || pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                {
                    pTarget.SetPixel(x, y, pixel);
                }
            }
        }
        pSource.UnlockBits();
        pTarget.UnlockBits();
    }
}
