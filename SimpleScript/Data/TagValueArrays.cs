using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class TagValueArrays(Token? from, Word name, int level) : Token(from, name, level)
{
    public List<List<KeyValuePair<Word, List<Word>>>> Value { get; } = [];

    public void Append(Word value)
    {
        Value.LastOrDefault()?.LastOrDefault().Value.Add(value);
    }

    public void AppendTag(Word value)
    {
        Value.LastOrDefault()?.Add(new(value, []));
    }

    public void AppendNew(Word value)
    {
        Value.Add([new(value, [])]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValueArrays(Level, Name.Text, Value, true)
            .ToString();
    }
}
