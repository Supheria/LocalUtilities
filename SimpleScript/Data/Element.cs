using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Text;

namespace LocalUtilities.SimpleScript.Data;

public abstract class Element(/*Element? from, */Word name, Word @operator, Word tag, int level)
{
    //public Element? From { get; } = from;

    public Word Name { get; } = name;

    public Word Operator { get; set; } = @operator;

    public Word Tag { get; set; } = tag;

    public int Level { get; } = level;

    public override abstract string ToString();
}
