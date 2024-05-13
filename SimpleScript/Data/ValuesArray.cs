using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ValuesArray(Token? from, Word name, int level) : Token(from, name, level)
{
    public List<List<Word>> Value { get; } = [];

    public void Append(Word value)
    {
        Value.LastOrDefault()?.Add(value);
    }

    public void AppendNew(Word value)
    {
        Value.Add([value]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendValuesArray(Level, Name.Text, Value, true)
            .ToString();
    }
}
