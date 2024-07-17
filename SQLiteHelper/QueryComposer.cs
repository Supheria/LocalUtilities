using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper;

internal class QueryComposer
{
    StringBuilder Query { get; } = new();

    public QueryComposer Finish()
    {
        Query.Append(SignCollection.Semicolon);
        return this;
    }

    public QueryComposer Append(Volume volume)
    {
        Query.Append(volume);
        return this;
    }

    public QueryComposer AppendFields(Field[] fields)
    {
        Query.Append(SignCollection.OpenParenthesis)
            .AppendJoin(SignCollection.Comma, fields, (sb, field) =>
            {
                sb.Append(field);
            })
            .Append(SignCollection.CloseParenthesis);
        return this;
    }

    public QueryComposer AppendValues(Volume[] values)
    {
        Append(Keywords.Values);
        Query.Append(SignCollection.OpenParenthesis)
            .AppendJoin(SignCollection.Comma, values, (sb, value) =>
            {
                sb.Append(value);
            })
            .Append(SignCollection.CloseParenthesis);
        return this;
    }

    public QueryComposer AppendCondition(Condition condition)
    {
        Append(Keywords.Where);
        Query.Append(condition);
        return this;
    }

    public QueryComposer AppendConditions(Condition[] conditioins, Condition.Combos combo)
    {
        Append(Keywords.Where);
        Query.AppendJoin(combo.ToKeywords().ToString(), conditioins, (sb, condition) =>
        {
            Query.Append(condition);
        });
        return this;
    }

    public QueryComposer AppendColumnFields(Assignment[] assignments)
    {
        Query.AppendJoin(SignCollection.Comma, assignments, (sb, assignment) =>
        {
            sb.Append(assignment);
        });
        return this;
    }

    public override string ToString()
    {
        return Query.ToString();
    }
}
