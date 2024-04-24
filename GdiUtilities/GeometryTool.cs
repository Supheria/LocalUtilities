using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.GdiUtilities;

public static class GeometryTool
{
    /// <summary>
    /// 判断点是否在多边形内.
    /// <para>
    /// ----------原理----------
    /// </para>
    /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
    /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
    /// 所以，我们可以顺序考虑多边形的每条边，求出交点的总个数。还有一些特殊情况要考虑。假如考虑边(P1,P2)，
    /// <para>
    /// 1)如果射线正好穿过P1或者P2,那么这个交点会被算作2次，处理办法是如果P的从坐标与P1,P2中较小的纵坐标相同，则直接忽略这种情况
    /// </para>
    /// <para>
    /// 2)如果射线水平，则射线要么与其无交点，要么有无数个，这种情况也直接忽略。
    /// </para>
    /// <para>
    /// 3)如果射线竖直，而P0的横坐标小于P1,P2的横坐标，则必然相交。
    /// </para>
    /// <para>
    /// 4)再判断相交之前，先判断P是否在边(P1,P2)的上面，如果在，则直接得出结论：P再多边形内部。
    /// </para>
    /// </summary>
    /// <param name="point">要判断的点</param>
    /// <param name="polygon">多边形的顶点</param>
    /// <returns></returns>
    public static bool PointInPolygon(this (int X, int Y) point, (int X, int Y)[] polygon)
    {
        var inside = false;
        //
        // 第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...
        //
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i, i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[j];
            //
            // p2在射线之上
            //
            if (point.Y < p2.Y)
            {
                //
                // p1正好在射线中或者射线下方
                //
                if (p1.Y <= point.Y)
                {
                    //
                    // 斜率判断,在P1和P2之间且在P1P2右侧
                    //
                    if ((point.Y - p1.Y) * (p2.X - p1.X) > (point.X - p1.X) * (p2.Y - p1.Y))
                        // 射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。
                        // 由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)
                        // 所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)
                        inside = (!inside);
                }
            }
            //
            // p2正好在射线中或者在射线下方，p1在射线上
            //
            else if (point.Y < p1.Y)
            {
                // 
                // 斜率判断,在P1和P2之间且在P1P2右侧
                //
                if ((point.Y - p1.Y) * (p2.X - p1.X) < (point.X - p1.X) * (p2.Y - p1.Y))
                    inside = (!inside);
            }
        }
        return inside;
    }

    public static Rectangle GetPolygonBounds(this (int X, int Y)[] polygon)
    {
        if (polygon.Length is 0)
            return new(0, 0, 0, 0);
        int left, right, top, bottom;
        left = right = polygon[0].X;
        top = bottom = polygon[0].Y;
        for (int i = 1; i < polygon.Length; i++)
        {
            var point = polygon[i];
            left = Math.Min(left, point.X);
            right = Math.Max(right, point.X);
            top = Math.Min(top, point.Y);
            bottom = Math.Max(bottom, point.Y);
        }
        return new(left, top, right - left, bottom - top);
    }
}
