using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsSerializer
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    public byte[] GetBuffer()
    {
        return StringBuilder.ToString().Select(c => (byte)c).ToArray();
    }

    public void WriteNameStart(string name)
    {
        StringBuilder.AppendNameStart(Level++, name);
    }

    public void WriteNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level);
    }

    public void WriteTag(string name, string tag)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, []);
    }

    public void WriteValues(string name, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, "_", values);
    }

    public void WriteTagValues(string name, string tag, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values);
    }

    public void WriteValuesArray(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray);
    }

    public void WriteTagValuesPairsArray(string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValuesPairsArray(Level, name, pairsArray);
    }
}
