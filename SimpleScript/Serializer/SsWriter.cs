using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public class SsWriter(bool writeIntoMultiLines)
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    bool NameAppended { get; set; } = false;

    public bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    public void AppendName(string name)
    {
        _ = StringBuilder.AppendName(Level, name, WriteIntoMultiLines);
        NameAppended = true;
    }

    public void AppendStart()
    {
        Level++;
        if (NameAppended)
            _ = StringBuilder.AppendStart(0, WriteIntoMultiLines);
        else
            _ = StringBuilder.AppendStart(Level, WriteIntoMultiLines);
        NameAppended = false;
    }

    public void AppendEnd()
    {
        _ = StringBuilder.AppendEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendValue(string value)
    {
        if (NameAppended)
            _ = StringBuilder.AppendValue(0, value, WriteIntoMultiLines);
        else
            _ = StringBuilder.AppendValue(Level, value, WriteIntoMultiLines);
        NameAppended = false;
    }
}
