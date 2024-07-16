using LocalUtilities.SimpleScript.Data.Convert;
using LocalUtilities.SimpleScript.Parser;
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

    private static void WriteUtf8File(string text, string filePath)
    {
        using var file = File.Create(filePath);
        file.Write(Utf8_BOM);
        using var streamWriter = new StreamWriter(file, Encoding.UTF8);
        streamWriter.Write(text);
        streamWriter.Close();
    }

    private static byte[] ReadFileBuffer(string filePath)
    {
        if (!File.Exists(filePath))
            throw SsParseException.CannotOpenFile(filePath);
        byte[] buffer;
        using var file = File.OpenRead(filePath);
        if (file.ReadByte() == Utf8_BOM[0] && file.ReadByte() == Utf8_BOM[1] && file.ReadByte() == Utf8_BOM[2])
        {
            buffer = new byte[file.Length - 3];
            _ = file.Read(buffer, 0, buffer.Length);
        }
        else
        {
            file.Seek(0, SeekOrigin.Begin);
            buffer = new byte[file.Length];
            _ = file.Read(buffer, 0, buffer.Length);
        }
        return buffer;
    }
}
