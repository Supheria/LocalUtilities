using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ElementScope(Element? from, Word name, Word @operator, Word tag, int level) : Element(from, name, @operator, tag, level)
{
    public Dictionary<string, List<Element>> Property { get; } = [];

    public void Append(Element property)
    {
        if (property is NullElement)
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
            .AppendJoin("", Property.Values.ToList(), (sb, property) =>
            {
                sb.Append(property.ToString());
            })
            .AppendNameEnd(Level, true)
            .ToString();
    }
}
