using LocalUtilities.SimpleScript.Data;

namespace LocalUtilities.SimpleScript.Parser;

internal class ParseTree
{
    const char OpenBrace = '{';
    const char CloseBrace = '}';
    const char Equal = '=';
    const char Greater = '>';
    const char Less = '<';

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

    List<Element> Arrays { get; } = [];

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
                    case OpenBrace:
                    case CloseBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    default:
                        Step = Steps.Name;
                        Name = token.Submit();
                        return this;
                }
            case Steps.Name: // 1
                switch (ch)
                {
                    case Equal:
                    case Greater:
                    case Less:
                        Step = Steps.Operator;
                        Operator = token.Submit();
                        return this;
                    default:
                        Builder = new ElementScope(From?.Builder, Name, Operator, Tag, Level);
                        Done();
                        return From;
                }
            case Steps.Operator: // 2
                switch (ch)
                {
                    case CloseBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case OpenBrace:
                        if (Operator.Text[0] != Equal)
                            throw SsParseExceptions.UnexpectedOperator(new(Operator.Text, Operator.Line, Operator.Column), Step.ToString());
                        Step = Steps.OperatorOn;
                        token.Submit();
                        return this;
                    default:
                        Step = Steps.Tag;
                        Tag = token.Submit();
                        return this;
                }
            case Steps.Tag: // 3
                switch (ch)
                {
                    case OpenBrace:
                        if (Operator.Text[0] != Equal)
                            throw SsParseExceptions.UnexpectedOperator(new(Operator.Text, Operator.Line, Operator.Column), Step.ToString());
                        Step = Steps.OperatorOn;
                        token.Submit();
                        return this;
                    default:
                        Builder = new ElementScope(From?.Builder, Name, Operator, Tag, Level);
                        Done();
                        // element.Get(); // leave element to next tree
                        return From;
                }
            case Steps.OperatorOn: // 5
                switch (ch)
                {
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case CloseBrace:
                        token.Submit();
                        Builder = new ElementScope(From?.Builder, Name, Operator, Tag, Level);
                        Done();
                        return From;
                    case OpenBrace:
                        Step = Steps.ArrayOn;
                        token.Submit();
                        return this;
                    default:
                        Step = Steps.Sub;
                        Builder = new ElementScope(From?.Builder, Name, Operator, Tag, Level);
                        return new(this, Level + 1);
                }
            case Steps.Sub: // 6
                switch (ch)
                {
                    case OpenBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case CloseBrace:
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
                    case OpenBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    case CloseBrace:
                        Step = Steps.ArrayOff;
                        token.Submit();
                        return this;
                    default:
                        Step = Steps.ArraySub;
                        Builder = new ElementScope(From?.Builder, Name, Operator, Tag, Level);
                        return new(this, Level + 1);
                }
            case Steps.ArrayOff: // 8
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.ArrayOn;
                        token.Submit();
                        return this;
                    case CloseBrace:
                        token.Submit();
                        Builder = new ElementArray(From?.Builder, Name, Operator, Tag, Level);
                        foreach (var scope in Arrays.Cast<ElementScope>())
                            ((ElementArray)Builder).Append(scope.Property);
                        Done();
                        return From;
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(token, Step.ToString());
                    default:
                        throw SsParseExceptions.UnexpectedValue(token, Step.ToString());
                }
            case Steps.ArraySub: // 9
                switch (ch)
                {
                    case CloseBrace:
                        Step = Steps.ArrayOff;
                        token.Submit();
                        Arrays.Add(Builder!);
                        return this;
                    case OpenBrace:
                        throw SsParseExceptions.UnexpectedDelimiter(token, Step.ToString());
                    case Equal:
                    case Greater:
                    case Less:
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
