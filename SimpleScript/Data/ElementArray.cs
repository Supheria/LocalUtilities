using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Data;

public class ElementArray(Element? from, Word name, Word @operator, Word tag, int level) : Element(from, name, @operator, tag, level)
{
    public List<Dictionary<string, List<Element>>> Properties { get; } = [];

    public void Append(Dictionary<string, List<Element>> property)
    {
        Properties.Add(property);
    }

    public override string ToString()
    {
        //return new StringBuilder()
        //    .AppendNameStart(Level, Name.Text, true)
        //    .AppendJoin("", Property.Values.ToList(), (sb, property) =>
        //    {
        //        sb.Append(property.ToString());
        //    })
        //    .AppendNameEnd(Level, true)
        //    .ToString();
        return "";
    }

}
