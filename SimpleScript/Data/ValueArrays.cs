using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ValueArrays(Token? from, Word name, int level) : Token(from, name, level)
{
    public List<List<string>> Value { get; } = [];

    public void Append(Word value)
    {
        Value.LastOrDefault()?.Add(value.Text);
    }

    public void AppendNew(Word value)
    {
        Value.Add([value.Text]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendValueArrays(Level, Name.Text, Value, true)
            .ToString();
    }
}
