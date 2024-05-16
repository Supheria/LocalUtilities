using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsWriter(bool writeIntoMultiLines)
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    internal bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    internal void AppendComment(string comment)
    {
        _ = StringBuilder.AppendComment(Level, comment, WriteIntoMultiLines);
    }

    internal void AppendNameStart(string name)
    {
        _ = StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    internal void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    internal void AppendArrayStart()
    {
        _ = StringBuilder.AppendArrayStart(Level++, WriteIntoMultiLines);
    }

    internal void AppendArrayEnd()
    {
        _ = StringBuilder.AppendArrayEnd(--Level, WriteIntoMultiLines);
    }

    internal void AppendValues(string name, List<string> value)
    {
        _ = StringBuilder.AppendValues(Level, name, value, WriteIntoMultiLines);
    }

    internal void AppendTagValues(string name, string tag, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values, WriteIntoMultiLines);
    }

    internal void AppendTag(string name, string tag)
    {
        _ = StringBuilder.AppendTag(Level, name, tag, WriteIntoMultiLines);
    }

    internal void AppendValuesArray(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray, WriteIntoMultiLines);
    }
}
