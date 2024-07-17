using LocalUtilities.TypeGeneral;

namespace LocalUtilities.SimpleScript.Parser;

internal class ParseTree
{
    private enum Steps
    {
        None,
        Name,
        Operator,
        SubOn,
        Tag,
        Sub,
        ArrayOn,
        ArrayOff,
        ArraySub,
    }

    Steps Step { get; set; } = Steps.None;

    Word Name { get; set; } = new();

    Word Operator { get; set; } = new();

    Word Value { get; set; } = new();

    List<Element> Array { get; } = [];

    Element? Builder { get; set; } = null;

    int Level { get; }

    internal ParseTree? From { get; }

    public ParseTree()
    {
        From = null;
        Level = 0;
    }

    public ParseTree(ParseTree from, int level)
    {
        From = from;
        Level = level;
        Step = Steps.None;
    }

    public Element? Submit()
    {
        var builder = Builder;
        Builder = null;
        return builder;
    }

    private void Done()
    {
        if (From is not null)
        {
            From.Append(Builder);
            Builder = null;
        }
    }

    private void Append(Element? element)
    {
        (Builder as Element)?.Append(element);
    }

    public ParseTree? Parse(Token token)
    {
        var ch = token.Head();
        switch (Step)
        {
            case Steps.None: // 0
                switch (ch)
                {
                    case SignTable.OpenBrace:
                    case SignTable.CloseBrace:
                        throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    default:
                        Step = Steps.Name;
                        Name = token.Submit();
                        Builder = new(new(), new(), Name, Level);
                        return this;
                }
            case Steps.Name: // 1
                switch (ch)
                {
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        Step = Steps.Operator;
                        Operator = token.Submit();
                        return this;
                    case SignTable.OpenBrace:
                        token.Submit();
                        Step = Steps.SubOn;
                        Value = Name;
                        Name = new();
                        return this;
                    default:
                        Done();
                        return From;
                }
            case Steps.Operator: // 2
                switch (ch)
                {
                    case SignTable.CloseBrace:
                        throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    case SignTable.OpenBrace:
                        token.Submit();
                        if (Operator.Text[0] != SignTable.Equal)
                            throw SsParseException.UnexpectedOperator(Operator, Step.ToString());
                        Step = Steps.SubOn;
                        return this;
                    default:
                        Value = token.Submit();
                        Step = Steps.Tag;
                        return this;
                }
            case Steps.Tag: // 3
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        token.Submit();
                        if (Operator.Text[0] != SignTable.Equal)
                            throw SsParseException.UnexpectedOperator(Operator, Step.ToString());
                        Step = Steps.SubOn;
                        return this;
                    default:
                        Builder = new Element(/*From?.Builder, */Name, Operator, Value, Level);
                        Done();
                        // element.Get(); // leave element to next tree
                        return From;
                }
            case Steps.SubOn: // 5
                switch (ch)
                {
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    case SignTable.CloseBrace:
                        token.Submit();
                        Builder = new Element(/*From?.Builder, */Name, Operator, Value, Level);
                        Done();
                        return From;
                    case SignTable.OpenBrace:
                        token.Submit();
                        Step = Steps.ArrayOn;
                        return this;
                    default:
                        Step = Steps.Sub;
                        Builder = new Element(/*From?.Builder, */Name, Operator, Value, Level);
                        return new(this, Level + 1);
                }
            case Steps.Sub: // 6
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    case SignTable.CloseBrace:
                        token.Submit();
                        Done();
                        return From;
                    default:
                        Step = Steps.Sub;
                        return new(this, Level + 1);
                }
            case Steps.ArrayOn: // 7
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    case SignTable.CloseBrace:
                        token.Submit();
                        Step = Steps.ArrayOff;
                        return this;
                    default:
                        Step = Steps.ArraySub;
                        Builder = new Element(/*From?.Builder, */Name, Operator, Value, Level);
                        return new(this, Level + 1);
                }
            case Steps.ArrayOff: // 8
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        Step = Steps.ArrayOn;
                        token.Submit();
                        return this;
                    case SignTable.CloseBrace:
                        token.Submit();
                        var scope = new Element(/*From?.Builder, */Name, Operator, Value, Level);
                        scope.AppendArray(Array);
                        Builder = scope;
                        Done();
                        return From;
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    default:
                        throw SsParseException.UnexpectedValue(token, Step.ToString());
                }
            case Steps.ArraySub: // 9
                switch (ch)
                {
                    case SignTable.CloseBrace:
                        Step = Steps.ArrayOff;
                        token.Submit();
                        Array.Add(Builder!);
                        return this;
                    case SignTable.OpenBrace:
                        throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseException.UnexpectedOperator(token, Step.ToString());
                    default:
                        Step = Steps.ArraySub;
                        return new(this, Level + 1);
                }
            default:
                throw new SsParseException("Out range of parse tree.");
        }
    }
}
