using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class TagValue(Element? from, Word name, Word @operator, Word tag, int level) : Element(from, name, @operator, tag, level)
{
    public Word Operator { get; } = @operator;

    public Word Tag { get; } = tag;

    public List<string> Value { get; } = [];

    public void Append(Word value)
    {
        Value.Add(value.Text);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValue(Level, Name.Text, Tag.Text, Value, true)
            .ToString();
    }
}
