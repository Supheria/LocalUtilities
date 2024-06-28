using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public class ElementArray(Word name, Word @operator, Word tag, int level) : Element(name, @operator, tag, level)
{
    public List<Dictionary<string, List<Element>>> Properties { get; } = [];

    public void Append(Dictionary<string, List<Element>> property)
    {
        Properties.Add(property);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendNameStart(Level, Name.Text, true)
            .AppendJoin(SignTable.Empty, Properties, (sb, elements) =>
            {
                sb.AppendArrayStart(Level, true)
                .AppendJoin(SignTable.Empty, elements.Values.ToList(), (sb, property) =>
                {
                    sb.Append(property.ToString());
                })
                .AppendArrayEnd(Level, true);
            })
            .AppendNameEnd(Level, true)
            .ToString();
    }
}
