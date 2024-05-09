using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class TagValuesPairsArray(Token? from, string name, int level) : Token(from, name, level)
{
    public List<List<KeyValuePair<string, List<string>>>> Value { get; } = [];

    public void Append(string value)
    {
        Value.LastOrDefault()?.LastOrDefault().Value.Add(value);
    }

    public void AppendTag(string value)
    {
        Value.LastOrDefault()?.Add(new(value, []));
    }

    public void AppendNew(string value)
    {
        Value.Add([new(value, [])]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValuesPairsArray(Level, Name, Value)
            .ToString();
    }
}