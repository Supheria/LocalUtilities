using LocalUtilities.SimpleScript.Common;

namespace LocalUtilities.SimpleScript.Serializer;

internal abstract class SsWriter(bool writeIntoMultiLines, SignTable signTable)
{
    int Level { get; set; } = 0;

    bool NameAppended { get; set; } = false;

    bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    SignTable SignTable { get; } = signTable;

    protected abstract void WriteString(string str);

    public void AppendName(string name)
    {
        var str = SsFormatter.GetName(Level, name, WriteIntoMultiLines, SignTable);
        WriteString(str);
        NameAppended = true;
    }

    public void AppendStart()
    {
        Level++;
        string str;
        if (NameAppended)
            str = SsFormatter.GetStart(0, WriteIntoMultiLines, SignTable);
        else
            str = SsFormatter.GetStart(Level, WriteIntoMultiLines, SignTable);
        WriteString(str);
        NameAppended = false;
    }

    public void AppendEnd()
    {
        var str = SsFormatter.GetEnd(--Level, WriteIntoMultiLines, SignTable);
        WriteString(str);
    }

    public void AppendValue(string value)
    {
        string str;
        if (NameAppended)
            str = SsFormatter.GetValue(0, value, WriteIntoMultiLines, SignTable);
        else
            str = SsFormatter.GetValue(Level, value, WriteIntoMultiLines, SignTable);
        WriteString(str);
        NameAppended = false;
    }

    public void AppendUnquotedValue(string value)
    {
        var str = SsFormatter.GetUnquotedValue(value, WriteIntoMultiLines);
        WriteString(str);
    }
}
