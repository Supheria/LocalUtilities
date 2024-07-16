using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.TypeGeneral;

namespace LocalUtilities.SimpleScript.Parser;

internal class ParseTree
{
    private enum Steps
    {
        None,
        Name,
        Operator,
        OperatorOn,
        Tag,
        Sub,
        ArrayOn,
        ArrayOff,
        ArraySub,
    }

    Steps Step { get; set; } = Steps.None;

    Word Name { get; set; } = new();

    Word Operator { get; set; } = new();

    Word Tag { get; set; } = new();

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
        (Builder as ElementScope)?.Append(element);
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
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    default:
                        Step = Steps.Name;
                        Name = token.Submit();
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
                        Step = Steps.OperatorOn;
                        Tag = Name;
                        Name = new();
                        return this;
                    default:
                        //
                        // read as value
                        //
                        Tag = Name;
                        Name = new();
                        Builder = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
                        Done();
                        return From;
                }
            case Steps.Operator: // 2
                switch (ch)
                {
                    case SignTable.CloseBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case SignTable.OpenBrace:
                        token.Submit();
                        if (Operator.Text[0] != SignTable.Equal)
                            throw SsParseExceptions.UnexpectedOperator(Operator, Step.ToString());
                        Step = Steps.OperatorOn;
                        return this;
                    default:
                        Tag = token.Submit();
                        Step = Steps.Tag;
                        return this;
                }
            case Steps.Tag: // 3
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        token.Submit();
                        if (Operator.Text[0] != SignTable.Equal)
                            throw SsParseExceptions.UnexpectedOperator(Operator, Step.ToString());
                        Step = Steps.OperatorOn;
                        return this;
                    default:
                        Builder = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
                        Done();
                        // element.Get(); // leave element to next tree
                        return From;
                }
            case Steps.OperatorOn: // 5
                switch (ch)
                {
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case SignTable.CloseBrace:
                        token.Submit();
                        Builder = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
                        Done();
                        return From;
                    case SignTable.OpenBrace:
                        token.Submit();
                        Step = Steps.ArrayOn;
                        return this;
                    default:
                        Step = Steps.Sub;
                        Builder = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
                        return new(this, Level + 1);
                }
            case Steps.Sub: // 6
                switch (ch)
                {
                    case SignTable.OpenBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
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
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case SignTable.CloseBrace:
                        token.Submit();
                        Step = Steps.ArrayOff;
                        return this;
                    default:
                        Step = Steps.ArraySub;
                        Builder = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
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
                        var scope = new ElementScope(/*From?.Builder, */Name, Operator, Tag, Level);
                        scope.AppendArray(Array);
                        Builder = scope;
                        Done();
                        return From;
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    default:
                        throw SsParseExceptions.UnexpectedValue(token, Step.ToString());
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
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case SignTable.Equal:
                    case SignTable.Greater:
                    case SignTable.Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    default:
                        Step = Steps.ArraySub;
                        return new(this, Level + 1);
                }
            default:
                throw new SsParseExceptions("Out range of parse tree.");
        }
    }
}
