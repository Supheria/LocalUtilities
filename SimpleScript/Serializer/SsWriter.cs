using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public class SsWriter(Stream stream, bool writeIntoMultiLines, SignTable? signTable, Encoding encoding) : IDisposable
{
    SsFormatter Formatter { get; } = new(stream, writeIntoMultiLines, signTable ?? new DefaultSignTable(), encoding);

    int Level { get; set; } = 0;

    bool NameAppended { get; set; } = false;

    public bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public void AppendName(string name)
    {
        Formatter.WriteName(Level, name);
        NameAppended = true;
    }

    public void AppendStart()
    {
        Level++;
        if (NameAppended)
            Formatter.WriteStart(0);
        else
            Formatter.WriteStart(Level);
        NameAppended = false;
    }

    public void AppendEnd()
    {
        Formatter.WriteEnd(--Level);
    }

    public void AppendValue(string value)
    {
        if (NameAppended)
            Formatter.WriteValue(0, value);
        else
            Formatter.WriteValue(Level, value);
        NameAppended = false;
    }

    public void AppendUnquotedValue(string value)
    {
        Formatter.WriteUnquotedValue(value);
    }

    public void Dispose()
    {
        Formatter.Dispose();
        GC.SuppressFinalize(this);
    }
}
