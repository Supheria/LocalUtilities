namespace LocalUtilities.GdiUtilities;

public static class GeometryTool
{
    public static bool PolygonContainsPoint(this (int X, int Y)[] polygon, (int X, int Y) point)
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
