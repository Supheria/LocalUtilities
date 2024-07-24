using LocalUtilities.SimpleScript;
using LocalUtilities.SimpleScript.Common;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeToolKit;
using LocalUtilities.TypeToolKit.Text;
using System.Data.SQLite;
using System.Text;

namespace LocalUtilities.SQLiteHelper;

internal static class QueryHelper
{
    public static string ToQuoted(this string volume)
    {
        return new StringBuilder()
            .Append(Keywords.Quote)
            .Append(volume)
            .Append(Keywords.Quote)
            .ToString();
    }

    public static StringBuilder AppendFieldsName(this StringBuilder query, Field[] fields)
    {
        return query.AppendJoin(Keywords.Comma.ToString(), fields, (sb, field) =>
         {
             sb.Append(field.Name.ToQuoted())
             .Append(ConvertType(field.Type));
             if (field.IsPrimaryKey)
                 sb.Append(Keywords.Blank)
                 .Append(Keywords.PrimaryKey);
         });
    }

    private static Keywords ConvertType(Type type)
    {
        if (type == TypeTable.Byte ||
            type == TypeTable.Char ||
            type == TypeTable.Short ||
            type == TypeTable.Int ||
            type == TypeTable.Long ||
            type == TypeTable.DateTime)
            return Keywords.Integer;
        if (type == TypeTable.Float ||
            type == TypeTable.Double)
            return Keywords.Real;
        return Keywords.Text;
    }

    public static Func<int, string> ConvertType(this SQLiteDataReader reader, Type type)
    {
        if (type == TypeTable.Byte)
            return i => reader.GetByte(i).ToString();
        if (type == TypeTable.Char)
            return i => reader.GetChar(i).ToString();
        if (type == TypeTable.Short)
            return i => reader.GetInt16(i).ToString();
        if (type == TypeTable.Int)
            return i => reader.GetInt32(i).ToString();
        if (type == TypeTable.Long ||
            type == TypeTable.DateTime)
            return i => reader.GetInt64(i).ToString();
        if (type == TypeTable.Float)
            return i => reader.GetFloat(i).ToString();
        if (type == TypeTable.Double)
            return i => reader.GetDouble(i).ToString();
        return reader.GetString;
    }

    public static StringBuilder AppendConditions(this StringBuilder query, Condition[] conditions, Condition.Combo combo, SignTable signTable)
    {
        if (conditions.Length < 1)
            return query;
        var comboWord = combo switch
        {
            Condition.Combo.Or => Keywords.Or,
            Condition.Combo.And => Keywords.And,
            _ => Keywords.Or
        };
        return query.Append(Keywords.Where)
            .AppendJoin(comboWord.ToString(), conditions, (sb, condition) =>
            {
                var value = SerializeTool.Serialize(condition.Value, new(), signTable, false);
                sb.Append(condition.Key.ToQuoted())
                .Append(condition.Operate)
                .Append(value.ToQuoted());
            });
    }
}
