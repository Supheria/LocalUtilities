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
        //
        // Start
        //
        None,
        Name,
        Operator,
        OperatorOn,
        Value,
        Tag,
        //
        // Sub
        //
        Sub,
        SubName,
        //
        // Array
        //
        Array,
        ArrayOff,
        ArrayName,
        ArrayOn,
        //
        // ValueArray
        //
        ValueArray,
        ValueArrayOff,
        ValueArrayOn,
        ValueArrayName,
        //
        // TagArray
        //
        TagArray,
        TagArrayValue,
        TagArrayValueOff,
        TagArrayOff,
        TagArrayName,
        TagArrayOn,
    }

    Steps Step { get; set; } = Steps.None;

    Word Name { get; set; } = new();

    Word Operator { get; set; } = new();

    Word Value { get; set; } = new();

    Word Array { get; set; } = new();

    Token Builder { get; set; } = new NullToken();

    int Level { get; }

    internal ParseTree? From { get; }

    public ParseTree()
    {
        From = null;
        Level = 0;
    }

    public ParseTree(ParseTree from, int level, Word name, Word @operator)
    {
        From = from;
        Level = level;
        Name = name;
        Operator = @operator;
        Step = Steps.Operator;
    }

    public Token DisposableGet()
    {
        var builder = Builder;
        Builder = new NullToken();
        return builder;
    }

    public void Done()
    {
        if (From is not null)
        {
            From.Append(Builder);
            Builder = new NullToken();
        }
    }

    private void Append(Token token)
    {
        (Builder as Scope)?.Append(token);
    }

    public ParseTree? Parse(Element element)
    {
        var ch = element.Head();
        switch (Step)
        {
            #region ==== Start ====

            case Steps.None: // 0
                switch (ch)
                {
                    case OpenBrace:
                    case CloseBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedName(element);
                    default:
                        Step = Steps.Name;
                        Name = element.Get();
                        return this;
                }
            case Steps.Name: // 1
                switch (ch)
                {
                    case Equal:
                    case Greater:
                    case Less:
                        Step = Steps.Operator;
                        Operator = element.Get();
                        return this;
                    default:
                        throw SsParseExceptions.UnexpectedOperator(element);
                }
            case Steps.Operator: // 2
                switch (ch)
                {
                    case CloseBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case OpenBrace:
                        if (Operator.Text[0] != Equal)
                            throw SsParseExceptions.UnexpectedOperator(element);
                        else
                        {
                            Step = Steps.OperatorOn;
                            element.Get();
                            return this;
                        }
                    default:
                        Step = Steps.Tag;
                        Builder = new TagValue(From?.Builder, Name, Level, Operator, element.Get());
                        return this;
                }
            case Steps.Tag: // 3
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.Value;
                        element.Get();
                        return this;
                    default:
                        Done();
                        // element.Get(); // leave element to next tree
                        return From;
                }
            case Steps.Value: // 4
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        element.Get();
                        Done();
                        return From;
                    default:
                        Step = Steps.Value;
                        ((TagValue)Builder).Append(element.Get());
                        return this;
                }
            case Steps.OperatorOn: // 5
                switch (ch)
                {
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        element.Get();
                        //
                        // some case like xyz = {}
                        //
                        Builder = Builder is NullToken ? new Scope(From?.Builder, Name, Level) : Builder;
                        Done();
                        return From;
                    case OpenBrace:
                        Step = Steps.Array;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.Sub | Steps.Name;
                        Value = element.Get();
                        Builder = new Scope(From?.Builder, Name, Level);
                        return this;
                }

            #endregion


            #region ==== Sub ====

            case Steps.SubName: // 6
                switch (ch)
                {
                    case OpenBrace:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        ((Scope)Builder).Append(new(From?.Builder, Value, Level + 1));
                        element.Get();
                        Done();
                        return From;
                    case Equal:
                    case Greater:
                    case Less:
                        Step = Steps.Sub;
                        return new(this, Level + 1, Value, element.Get());
                    default:
                        ((Scope)Builder).Append(new(From?.Builder, Value, Level + 1));
                        Value = element.Get();
                        return this;
                }
            case Steps.Sub: // 7
                switch (ch)
                {
                    case OpenBrace:
                    case CloseBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        element.Get();
                        Done();
                        return From;
                    default:
                        Step = Steps.SubName;
                        Value = element.Get();
                        return this;
                }

            #endregion


            #region ==== Array ====

            case Steps.Array: // 8
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        Step = Steps.ArrayOff;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.ArrayName;
                        Array = element.Get();
                        return this;
                }
            case Steps.ArrayOff: // 9
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.Array;
                        element.Get();
                        return this;
                    case CloseBrace:
                        element.Get();
                        //
                        // some case like xyz = {{}}
                        //
                        Builder = Builder is NullToken ? new ValueArrays(From?.Builder, Name, Level) : Builder;
                        Done();
                        return From;
                    default:
                        throw SsParseExceptions.UnexpectedArraySyntax(element);
                }
            case Steps.ArrayName: // 10
                switch (ch)
                {
                    case OpenBrace:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(element);
                    case Equal:
                        Step = Steps.TagArray;
                        Builder = new TagValueArrays(From?.Builder, Name, Level);
                        ((TagValueArrays)Builder).AppendNew(Array);
                        element.Get();
                        return this;
                    case CloseBrace:
                        Step = Steps.ValueArrayOff;
                        Builder = new ValueArrays(From?.Builder, Name, Level);
                        ((ValueArrays)Builder).AppendNew(Array);
                        element.Get();
                        return this;
                    default:
                        Step = Steps.ValueArray;
                        Builder = new ValueArrays(From?.Builder, Name, Level);
                        ((ValueArrays)Builder).AppendNew(Array);
                        ((ValueArrays)Builder).Append(element.Get());
                        return this;
                }

            #endregion


            #region ==== ValueArray ====

            case Steps.ValueArray: // 11
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        Step = Steps.ValueArrayOff;
                        element.Get();
                        return this;
                    default:
                        ((ValueArrays)Builder).Append(element.Get());
                        return this;
                }
            case Steps.ValueArrayOff: // 12
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.ValueArrayOn;
                        element.Get();
                        return this;
                    case CloseBrace:
                        element.Get();
                        Done();
                        return From;
                    default:
                        throw SsParseExceptions.UnexpectedArraySyntax(element);
                }
            case Steps.ValueArrayOn: // 13
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        Step = Steps.ValueArrayOff;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.ValueArrayName;
                        ((ValueArrays)Builder).AppendNew(element.Get());
                        return this;
                }
            case Steps.ValueArrayName: // 14
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedArrayType(element);
                    case CloseBrace:
                        Step = Steps.ValueArrayOff;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.ValueArray;
                        ((ValueArrays)Builder).Append(element.Get());
                        return this;
                }

            #endregion


            #region ==== TagArray ====

            case Steps.TagArray: // 15
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.TagArrayValue;
                        element.Get();
                        return this;
                    default:
                        throw SsParseExceptions.UnexpectedArrayType(element);
                }
            case Steps.TagArrayValue: // 16
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedValue(element);
                    case CloseBrace:
                        Step = Steps.TagArrayValueOff;
                        element.Get();
                        return this;
                    default:
                        element.Get();
                        ((TagValueArrays)Builder).Append(element.Get());
                        return this;
                }
            case Steps.TagArrayValueOff: // 17
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedName(element);
                    case CloseBrace:
                        Step = Steps.TagArrayOff;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.TagArrayName;
                        ((TagValueArrays)Builder).AppendTag(element.Get());
                        return this;
                }
            case Steps.TagArrayOff: // 18
                switch (ch)
                {
                    case OpenBrace:
                        Step = Steps.TagArrayOn;
                        element.Get();
                        return this;
                    case CloseBrace:
                        element.Get();
                        Done();
                        return From;
                    default:
                        throw SsParseExceptions.UnexpectedArraySyntax(element);
                }
            case Steps.TagArrayOn: // 19
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedName(element);
                    case CloseBrace:
                        Step = Steps.TagArrayOff;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.TagArrayName;
                        ((TagValueArrays)Builder).AppendNew(element.Get());
                        return this;
                }
            case Steps.TagArrayName: // 20
                switch (ch)
                {
                    case Equal:
                        Step = Steps.TagArray;
                        element.Get();
                        return this;
                    case Greater:
                    case Less:
                        throw SsParseExceptions.UnexpectedOperator(element);
                    default:
                        throw SsParseExceptions.UnexpectedArrayType(element);
                }

            #endregion


            default:
                throw new SsParseExceptions("Out range of parse tree.");
        }
    }
}