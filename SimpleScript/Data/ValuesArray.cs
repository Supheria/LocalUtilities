using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ValuesArray(Token? from, string name, int level) : Token(from, name, level)
{
    public List<List<string>> Value { get; } = [];

    public void Append(string value)
    {
        Value.LastOrDefault()?.Add(value);
    }

    public void AppendNew(string value)
    {
        Value.Add([value]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendValuesArray(Level, Name, Value)
            .ToString();
    }
}