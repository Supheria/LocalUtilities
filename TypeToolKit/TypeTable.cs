using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeToolKit;

public sealed class TypeTable
{
    public static Type Byte { get; } = typeof(byte);
    public static Type Char { get; } = typeof(char);
    public static Type Bool { get; } = typeof(bool);
    public static Type Short { get; } = typeof(short);
    public static Type Int { get; } = typeof(int);
    public static Type Long { get; } = typeof(long);
    public static Type Float { get; } = typeof(float);
    public static Type Double { get; } = typeof(double);
    public static Type Enum { get; } = typeof(Enum);
    public static Type String { get; } = typeof(string);
    public static Type Point { get; } = typeof(Point);
    public static Type Rectangle { get; } = typeof(Rectangle);
    public static Type Size { get; } = typeof(Size);
    public static Type Color { get; } = typeof(Color);
    public static Type DateTime { get; } = typeof(DateTime);
}
