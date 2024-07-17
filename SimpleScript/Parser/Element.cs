using LocalUtilities.SimpleScript.Serializer;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Text;

namespace LocalUtilities.SimpleScript.Parser;

public class Element(/*Element? from, */Word name, Word @operator, Word value, int level)
{
    //public Element? From { get; } = from;

    public Word Name { get; } = name;

    public Word Operator { get; } = @operator;

    public Word Value { get; } = value;

    public int Level { get; } = level;

    public Dictionary<string, List<Element>> Property { get; } = [];

    public void Append(Element? property)
    {
        if (property is null)
            throw SsParseException.NullProperty();
        if (property.Level != Level + 1)
            throw SsParseException.LevelDismatch();
        if (Property.TryGetValue(property.Name.Text, out var list))
            list.Add(property);
        else
            Property[property.Name.Text] = [property];
    }

    public void AppendArray(List<Element> array)
    {
        if (Property.TryGetValue("", out var list))
            list.AddRange(array);
        else
            Property[""] = array;
    }

    //public override string ToString()
    //{
    //    //return new StringBuilder()
    //    //    .AppendName(Level, Name.Text, false)
    //    //    .AppendStart(Level, false)
    //    //    .AppendJoin(SignTable.Empty, Property.Values.ToList(), (sb, property) =>
    //    //    {
    //    //        sb.Append(property.ToString());
    //    //    })
    //    //    .AppendEnd(Level, false)
    //    //    .ToString();
    //}
}
