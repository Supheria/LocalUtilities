using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Data;

public abstract class Element(/*Element? from, */Word name, Word @operator, Word tag, int level)
{
    //public Element? From { get; } = from;

    public Word Name { get; } = name;

    public Word Operator { get; set; } = @operator;

    public Word Value { get; set; } = tag;

    public int Level { get; } = level;

    public override abstract string ToString();
}
