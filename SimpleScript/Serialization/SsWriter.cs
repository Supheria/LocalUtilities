using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsWriter(bool writeIntoMultiLines)
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    public bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    public void AppendComment(string comment)
    {
        _ = StringBuilder.AppendComment(Level, comment, WriteIntoMultiLines);
    }

    public void AppendNameStart(string name)
    {
        _ = StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    public void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendArrayStart(string? tag)
    {
        _ = StringBuilder.AppendArrayStart(Level++, tag, WriteIntoMultiLines);
    }

    public void AppendArrayEnd()
    {
        _ = StringBuilder.AppendArrayEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendValues(string name, List<string> value)
    {
        _ = StringBuilder.AppendValues(Level, name, value, WriteIntoMultiLines);
    }

    public void AppendTagValues(string name, string tag, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values, WriteIntoMultiLines);
    }

    public void AppendTag(string name, string tag)
    {
        _ = StringBuilder.AppendTag(Level, name, tag, WriteIntoMultiLines);
    }

    public void AppendValuesArray(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray, WriteIntoMultiLines);
    }
}
