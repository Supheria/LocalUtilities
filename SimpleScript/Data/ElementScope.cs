using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ElementScope(Word name, Word @operator, Word tag, int level) : Element(name, @operator, tag, level)
{
    public Dictionary<string, List<Element>> Property { get; } = [];

    public void Append(Element? property)
    {
        if (property is null)
            throw new SsParseExceptions("property to append cannot be null");
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
            .AppendJoin(SignTable.Empty, Property.Values.ToList(), (sb, property) =>
            {
                sb.Append(property.ToString());
            })
            .AppendNameEnd(Level, true)
            .ToString();
    }
}
