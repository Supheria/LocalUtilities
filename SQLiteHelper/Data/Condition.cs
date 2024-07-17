using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

public class Condition(Volume key, Volume value, Condition.Operates operate)
{
    public enum Combos
    {
        Or,
        And,
    }

    public enum Operates
    {
        Equal,
        Less,
        Greater,
    }

    public Volume Key { get; } = key;

    public Volume Value { get; } = value;

    public Keywords Operate { get; } = operate switch
    {
        Operates.Equal => Keywords.Equal,
        Operates.Less => Keywords.Less,
        Operates.Greater => Keywords.Greater,
        _ => Keywords.Null
    };

    public override string ToString()
    {
        return new StringBuilder()
            .Append(Key)
            .Append(Operate)
            .Append(Value)
            .ToString();
    }
}

internal static class ConditionCombo
{
    public static Keywords ToKeywords(this Condition.Combos combo)
    {
        return combo switch
        {
            Condition.Combos.Or => Keywords.Or,
            Condition.Combos.And => Keywords.And,
            _ => Keywords.Null
        };
    }
}
