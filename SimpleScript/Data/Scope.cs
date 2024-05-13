using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class Scope(Token? from, Word name, int level) : Token(from, name, level)
{
    public Dictionary<string, List<Token>> Property { get; } = [];

    public void Append(Token property)
    {
        if (property is NullToken)
            return;
        if (property.Level != Level + 1)
            throw new SsParseExceptions("level mismatched of Appending in Scope");
        if (Property.TryGetValue(property.Name.Text, out var list))
            list.Add(property);
        else
            Property[property.Name.Text] = [property];
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendNameStart(Level, Name.Text, true)
            .AppendJoin('\0', Property.Values.ToList(), (sb, property) =>
            {
                sb.Append(property.ToString());
            })
            .AppendNameEnd(Level, true)
            .ToString();
    }
}
