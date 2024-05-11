using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class TagValues(Token? from, string name, int level, string @operator, string tag) : Token(from, name, level)
{
    public string Operator { get; } = @operator;

    public string Tag { get; } = tag;

    public List<string> Value { get; } = [];

    public void Append(string value)
    {
        Value.Add(value);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValues(Level, Name, Tag, Value, true)
            .ToString();
    }
}