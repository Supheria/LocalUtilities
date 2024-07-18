using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeToolKit.Convert;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace LocalUtilities.SimpleScript;

public partial class SerializeTool
{
    public static Type TByte { get; } = typeof(byte);
    public static Type TChar { get; } = typeof(char);
    public static Type TBool { get; } = typeof(bool);
    public static Type TShort { get; } = typeof(short);
    public static Type TInt { get; } = typeof(int);
    public static Type TLong { get; } = typeof(long);
    public static Type TFloat { get; } = typeof(float);
    public static Type TDouble { get; } = typeof(double);
    public static Type TEnum { get; } = typeof(Enum);
    public static Type TString { get; } = typeof(string);
    public static Type TPoint { get; } = typeof(Point);
    public static Type TRectangle { get; } = typeof(Rectangle);
    public static Type TSize { get; } = typeof(Size);
    public static Type TColor { get; } = typeof(Color);
    public static Type TDateTime { get; } = typeof(DateTime);

    static BindingFlags Authority { get; } = BindingFlags.Public /*| BindingFlags.NonPublic*/ | BindingFlags.Instance;

    static byte[] Utf8_BOM { get; } = [0xEF, 0xBB, 0xBF];
}
