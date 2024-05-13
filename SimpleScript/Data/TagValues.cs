using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class TagValues(Token? from, Word name, int level, Word @operator, Word tag) : Token(from, name, level)
{
    public Word Operator { get; } = @operator;

    public Word Tag { get; } = tag;

    public List<Word> Value { get; } = [];

    public void Append(Word value)
    {
        Value.Add(value);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValues(Level, Name.Text, Tag.Text, Value, true)
            .ToString();
    }
}
