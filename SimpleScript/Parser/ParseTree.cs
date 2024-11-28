using LocalUtilities.SimpleScript.Common;

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

    SignTable SignTable { get; }

    public ParseTree? From { get; }

    public bool IsDone { get; private set; } = false;

    public ParseTree(SignTable signTable)
    {
        SignTable = signTable;
        From = null;
        Level = 0;
    }

    public ParseTree(SignTable signTable, ParseTree from, int level)
    {
        SignTable = signTable;
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
        IsDone = true;
    }

    private void Append(Element? element)
    {
        Builder?.Append(element);
    }

    public ParseTree? Parse(Token token)
    {
        var ch = token.Head();
        switch (Step)
        {
            case Steps.None: // 0
                if (ch == SignTable.Close)
                    throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                if (ch == SignTable.Open)
                {
                    token.Submit();
                    Step = Steps.ArrayOn;
                    return this;
                }
                Step = Steps.Name;
                Name = token.Submit();
                Builder = new(new(), new(), Name, Level);
                return this;
            case Steps.Name: // 1
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                {
                    Step = Steps.Operator;
                    Operator = token.Submit();
                    return this;
                }
                if (ch == SignTable.Open)
                {
                    token.Submit();
                    Step = Steps.SubOn;
                    Value = Name;
                    Name = new();
                    return this;
                }
                Done();
                return From;
            case Steps.Operator: // 2
                if (ch == SignTable.Close)
                    throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                if (ch == SignTable.Open)
                {
                    token.Submit();
                    if (Operator.Text[0] != SignTable.Equal)
                        throw SsParseException.UnexpectedOperator(Operator, Step.ToString());
                    Step = Steps.SubOn;
                    return this;
                }
                Value = token.Submit();
                Step = Steps.Tag;
                return this;
            case Steps.Tag: // 3
                if (ch == SignTable.Open)
                {
                    token.Submit();
                    if (Operator.Text[0] != SignTable.Equal)
                        throw SsParseException.UnexpectedOperator(Operator, Step.ToString());
                    Step = Steps.SubOn;
                    return this;
                }
                Builder = new Element(Name, Operator, Value, Level);
                Done();
                // element.Get(); // leave element to next tree
                return From;
            case Steps.SubOn: // 5
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                if (ch == SignTable.Close)
                {
                    token.Submit();
                    Builder = new Element(Name, Operator, Value, Level);
                    Done();
                    return From;
                }
                if (ch == SignTable.Open)
                {
                    token.Submit();
                    Step = Steps.ArrayOn;
                    return this;
                }
                Step = Steps.Sub;
                Builder = new Element(Name, Operator, Value, Level);
                return new(SignTable, this, Level + 1);
            case Steps.Sub: // 6
                if (ch == SignTable.Open)
                    throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                if (ch == SignTable.Close)
                {
                    token.Submit();
                    Done();
                    return From;
                }
                Step = Steps.Sub;
                return new(SignTable, this, Level + 1);
            case Steps.ArrayOn: // 7
                if (ch == SignTable.Open)
                    throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                if (ch == SignTable.Close)
                {
                    token.Submit();
                    Step = Steps.ArrayOff;
                    return this;
                }
                Step = Steps.ArraySub;
                Builder = new Element(Name, Operator, Value, Level);
                return new(SignTable, this, Level + 1);
            case Steps.ArrayOff: // 8
                if (ch == SignTable.Open)
                {
                    Step = Steps.ArrayOn;
                    token.Submit();
                    return this;
                }
                if (ch == SignTable.Close)
                {
                    token.Submit();
                    var scope = new Element(Name, Operator, Value, Level);
                    scope.AppendArray(Array);
                    Builder = scope;
                    Done();
                    return From;
                }
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                throw SsParseException.UnexpectedValue(token, Step.ToString());
            case Steps.ArraySub: // 9
                if (ch == SignTable.Close)
                {
                    Step = Steps.ArrayOff;
                    token.Submit();
                    Array.Add(Builder!);
                    return this;
                }
                if (ch == SignTable.Open)
                    throw SsParseException.UnexpectedDelimiter(token, Step.ToString());
                if (ch == SignTable.Equal ||
                    ch == SignTable.Greater ||
                    ch == SignTable.Less)
                    throw SsParseException.UnexpectedOperator(token, Step.ToString());
                Step = Steps.ArraySub;
                return new(SignTable, this, Level + 1);
            default:
                throw new SsParseException("Out range of parse tree.");
        }
    }
}
