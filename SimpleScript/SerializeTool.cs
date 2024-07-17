using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeToolKit.Convert;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace LocalUtilities.SimpleScript;

public static partial class SerializeTool
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

    private static bool IsSimpleType(Type type)
    {
        return type == TByte ||
            type == TChar ||
            type == TBool ||
            type == TShort ||
            type == TInt ||
            type == TLong ||
            type == TFloat ||
            type == TDouble ||
            type == TString ||
            TEnum.IsAssignableFrom(type);
    }

    private static bool GetSimpleTypeConvert(Type type, [NotNullWhen(true)] out Func<string, object?>? convert)
    {
        if (type == TByte)
            convert = str => str.ToByte();
        else if (type == TChar)
            convert = str => str.ToChar();
        else if (type == TBool)
            convert = str => str.ToBool();
        else if (type == TShort)
            convert = str => str.ToShort();
        else if (type == TInt)
            convert = str => str.ToInt();
        else if (type == TLong)
            convert = str => str.ToLong();
        else if (type == TFloat)
            convert = str => str.ToFloat();
        else if (type == TDouble)
            convert = str => str.ToDouble();
        else if (type == TString)
            convert = str => str;
        else if (TEnum.IsAssignableFrom(type))
            convert = str => str.ToEnum(type);
        else
        {
            convert = null;
            return false;
        }
        return true;
    }
}
