using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsSerializer(bool writeIntoMultiLines)
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    public bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    public void AppendToken(string name)
    {
        StringBuilder.AppendToken(Level, name, WriteIntoMultiLines);
    }

    public void AppendNameStart(string name)
    {
        StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    public void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendTag(string name, string tag)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, [], WriteIntoMultiLines);
    }

    public void AppendValues(string name, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, "_", values, WriteIntoMultiLines);
    }

    public void AppendTagValues(string name, string tag, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values, WriteIntoMultiLines);
    }

    public void AppendValuesArray(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray, WriteIntoMultiLines);
    }

    public void AppendTagValuesPairsArray(string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValuesPairsArray(Level, name, pairsArray, WriteIntoMultiLines);
    }
}
