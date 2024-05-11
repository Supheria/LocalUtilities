using LocalUtilities.SimpleScript.Parser;
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
        _ = StringBuilder.AppendToken(Level, name, WriteIntoMultiLines);
    }

    public void AppendNameStart(string name)
    {
        _ = StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    public void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendTag(string name, string tag)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, [], WriteIntoMultiLines);
    }

    public void AppendValues(string name, List<Word> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, "_", values, WriteIntoMultiLines);
    }

    public void AppendTagValues(string name, string tag, List<Word> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values, WriteIntoMultiLines);
    }

    public void AppendValuesArray(string name, List<List<Word>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray, WriteIntoMultiLines);
    }

    public void AppendTagValuesPairsArray(string name, List<List<KeyValuePair<Word, List<Word>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValuesPairsArray(Level, name, pairsArray, WriteIntoMultiLines);
    }
}
