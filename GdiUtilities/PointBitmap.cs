using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.GdiUtilities;

/// <summary>
/// 指针法 Bitmap 颜色赋值，来源 https://www.cnblogs.com/ybqjymy/p/12897892.html
/// </summary>
public class PointBitmap
{
    /// <summary>
    /// 指向的源图片
    /// </summary>
    readonly Bitmap Source;

    /// <summary>
    /// 指针首地址
    /// </summary>
    nint IntPointer = nint.Zero;

    /// <summary>
    /// 数据存储结构
    /// </summary>
    BitmapData? BitmapData;

    /// <summary>
    /// 图片位深
    /// </summary>
    public int Depth { get; private set; }

    public PointBitmap(Bitmap source)
    {
        Source = source;
    }

    /// <summary>
    /// 根据源图片大小和位深设置并调用 Bitmap.LockBits（只做了对 32、24、8 位的处理）
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void LockBits()
    {
        Depth = Image.GetPixelFormatSize(Source.PixelFormat);
        Rectangle rect = new(0, 0, Source.Width, Source.Height);
        if (Depth is not (32 or 24 or 8))
            throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
        BitmapData = Source.LockBits(rect, ImageLockMode.ReadWrite, Source.PixelFormat);
        IntPointer = BitmapData.Scan0;
    }

    /// <summary>
    /// 调用 Bitmap.LockBits
    /// </summary>
    public void UnlockBits()
    {
        Source.UnlockBits(BitmapData!);
    }

    /// <summary>
    /// 返回指针指向的地址的数据（代换 Bitmap.GetPixel)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Color GetPixel(int x, int y)
    {
        unsafe
        {
            var ptr = (byte*)IntPointer;
            ptr += y * BitmapData!.Stride + x * (Depth >> 3);
            switch (Depth)
            {
                case 32:
                    return Color.FromArgb(ptr[3], ptr[2], ptr[1], ptr[0]);
                case 24:
                    return Color.FromArgb(ptr[2], ptr[1], ptr[0]);
                default:
                    {
                        int r = ptr[0];
                        return Color.FromArgb(r, r, r);
                    }
            }
        }
    }

    /// <summary>
    /// 设置指针指向的地址的数据（代换 Bitmap.SetPixel）
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="c"></param>
    public void SetPixel(int x, int y, Color c)
    {
        unsafe
        {
            var ptr = (byte*)IntPointer;
            ptr += y * BitmapData!.Stride + x * (Depth >> 3);
            switch (Depth)
            {
                case 32:
                    ptr[3] = c.A;
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                    break;
                case 24:
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                    break;
                case 8:
                    //ptr[2] = c.R;
                    //ptr[1] = c.G;
                    ptr[0] = c.B;
                    break;
            }
        }
    }
}
