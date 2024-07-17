using LocalUtilities.SimpleScript.Common;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public class SsFormatter(Stream stream, bool writeIntoMultiLines, SignTable signTable, Encoding encoding) : IDisposable
{
    Stream Stream { get; } = stream;

    bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    SignTable SignTable { get; } = signTable;

    Encoding Encoding { get; } = encoding;

    public void Dispose()
    {
        Stream.Flush();
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }

    private string ToQuoted(string str)
    {
        if (str is "")
            return new StringBuilder()
                .Append(SignTable.Quote)
                .Append(SignTable.Quote)
                .ToString();
        var sb = new StringBuilder();
        bool useQuote = false;
        foreach (var ch in str)
        {
            if (ch == SignTable.Escape ||
                ch == SignTable.Quote)
            {
                sb.Append(SignTable.Escape)
                        .Append(ch);
                useQuote = true;
            }
            else if (ch == SignTable.Tab ||
                ch == SignTable.Space ||
                ch == SignTable.Note ||
                ch == SignTable.Equal ||
                ch == SignTable.Greater ||
                ch == SignTable.Less ||
                ch == SignTable.Open ||
                ch == SignTable.Close ||
                ch == SignTable.Return ||
                ch == SignTable.NewLine ||
                ch == SignTable.Empty)
            {
                useQuote = true;
                sb.Append(ch);
                continue;
            }
            else
                sb.Append(ch);
        }
        if (useQuote)
            return new StringBuilder()
                .Append(SignTable.Quote)
                .Append(sb)
                .Append(SignTable.Quote)
                .ToString();
        return sb.ToString();
    }

    private StringBuilder AppendNewLine(StringBuilder sb)
    {
        if (WriteIntoMultiLines)
            return sb.AppendLine();
        else
            return sb;
    }

    private StringBuilder AppendTab(StringBuilder sb, int times)
    {
        if (WriteIntoMultiLines)
        {
            for (var i = 0; i < times; i++)
                sb.Append(SignTable.Space)
                    .Append(SignTable.Space)
                    .Append(SignTable.Space)
                    .Append(SignTable.Space);
        }
        return sb;
    }

    private byte[] Encode(StringBuilder sb)
    {
        return Encoding.GetBytes(sb.ToString());
    }

    public void AppendComment(int level, string comment)
    {
        if (WriteIntoMultiLines)
        {
            var sb = AppendTab(new(), level)
                .Append(SignTable.Note)
                .Append(comment);
            AppendNewLine(sb);
            Stream.Write(Encode(sb));
        }
    }

    public void WriteName(int level, string name)
    {
        var sb = AppendTab(new(), level)
            .Append(ToQuoted(name))
            .Append(SignTable.Equal);
        Stream.Write(Encode(sb));
    }

    public void WriteStart(int level)
    {
        var sb = AppendTab(new(), level)
            .Append(SignTable.Open);
        AppendNewLine(sb);
        Stream.Write(Encode(sb));
    }

    public void WriteEnd(int level)
    {
        var sb = AppendTab(new(), level)
            .Append(SignTable.Close);
        AppendNewLine(sb);
        Stream.Write(Encode(sb));
    }

    public void WriteValue(int level, string value)
    {
        var sb = AppendTab(new(), level)
            .Append(ToQuoted(value));
        if (value is not "")
            sb.Append(SignTable.Space);
        AppendNewLine(sb);
        Stream.Write(Encode(sb));
    }

    public void WriteUnquotedValue(string value)
    {
        var sb = new StringBuilder(value);
        AppendNewLine(sb);
        Stream.Write(Encode(sb));
    }
}
