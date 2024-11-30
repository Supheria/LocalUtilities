using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities;

public class Color
{
    public byte A { get; private set; }

    public byte R { get; private set; }

    public byte G { get; private set; }

    public byte B { get; private set; }

    public Color(byte r, byte g, byte b)
    {
        A = byte.MaxValue;
        R = r;
        G = g;
        B = b;
    }

    public Color(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public Color()
    {
        A = byte.MaxValue;
        R = byte.MaxValue;
        G = byte.MaxValue;
        B = byte.MaxValue;
    }

    public static Color FromName(string name)
    {
        // TODO: do parse
        return new();
    }

    public static Color Black = new(0, 0, 0);
    public static Color LightSalmon = new(255, 160, 122);
    public static Color DarkSlateGray = new(40, 79, 79);
    public static Color Gold = new(255, 215, 0);
    public static Color MediumPurple = new(147, 112, 219);
    public static Color ForestGreen = new(34, 139, 34);
    public static Color RoyalBlue = new(65, 105, 225);
    public static Color YellowGreen = new(154, 205, 50);
    public static Color LightYellow = new(255, 255, 224);
    public static Color LimeGreen = new(50, 205, 50);
    public static Color SkyBlue = new(135, 206, 235);
}
