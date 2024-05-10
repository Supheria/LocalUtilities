using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.StringUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class Scope(Token? from, string name, int level) : Token(from, name, level)
{
    public List<Token> Property { get; } = [];

    public void Append(Token property)
    {
        if (property is NullToken)
            return;
        if (property.Level != Level + 1)
            throw new SsParseExceptions("level mismatched of Appending in Scope");
        Property.Add(property);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendNameStart(Level, Name)
            .AppendJoin('\0', Property, (sb, property) =>
            {
                sb.Append(property.ToString());
            })
            .AppendNameEnd(Level)
            .ToString();
    }
}