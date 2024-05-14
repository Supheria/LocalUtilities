using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;
using System.Xml.Linq;

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

    internal void AppendToken(string name)
    {
        _ = StringBuilder.AppendToken(Level, name, WriteIntoMultiLines);
    }

    internal void AppendNameStart(string name)
    {
        _ = StringBuilder.AppendNameStart(Level++, name, WriteIntoMultiLines);
    }

    internal void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level, WriteIntoMultiLines);
    }

    internal void AppendValues(string name, IList<string> values)
    {
        _ = StringBuilder.AppendTagValue(Level, name, "_", values, WriteIntoMultiLines);
    }

    internal void AppendTagValues(string name, string tag, IList<string> values)
    {
        _ = StringBuilder.AppendTagValue(Level, name, tag, values, WriteIntoMultiLines);
    }

    internal void AppendValuesArray(string name, List<List<Word>> valuesArray)
    {
        _ = StringBuilder.AppendValueArray(Level, name, valuesArray, WriteIntoMultiLines);
    }

    internal void AppendTagValuesPairsArray(string name, List<List<KeyValuePair<Word, List<Word>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValueArrays(Level, name, pairsArray, WriteIntoMultiLines);
    }
}
