using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsSerializer
{
    StringBuilder StringBuilder { get; } = new();

    int Level { get; set; } = 0;

    public override string ToString()
    {
        return StringBuilder.ToString();
    }

    public void AppendToken(string token)
    {
        StringBuilder.Append(token);
    }

    public void AppendNameStart(string name)
    {
        StringBuilder.AppendNameStart(Level++, name);
    }

    public void AppendNameEnd()
    {
        _ = StringBuilder.AppendNameEnd(--Level);
    }

    public void AppendTag(string name, string tag)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, []);
    }

    public void AppendValues(string name, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, "_", values);
    }

    public void AppendTagValues(string name, string tag, List<string> values)
    {
        _ = StringBuilder.AppendTagValues(Level, name, tag, values);
    }

    public void AppendValuesArray(string name, List<List<string>> valuesArray)
    {
        _ = StringBuilder.AppendValuesArray(Level, name, valuesArray);
    }

    public void AppendTagValuesPairsArray(string name, List<List<KeyValuePair<string, List<string>>>> pairsArray)
    {
        _ = StringBuilder.AppendTagValuesPairsArray(Level, name, pairsArray);
    }
}
