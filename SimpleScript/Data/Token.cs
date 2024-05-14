using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class Token(Token? from, Word name, int level)
{
    public Token? From { get; } = from is NullToken ? null : from;

    public Word Name { get; } = name;

    public int Level { get; } = level;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendToken(Level, Name.Text, true)
            .ToString();
    }
}
