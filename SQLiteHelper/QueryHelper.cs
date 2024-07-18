using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper.Data;
using System.Text;
using LocalUtilities.TypeToolKit.Text;
using LocalUtilities.SimpleScript.Common;
using System.Data.SQLite;

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
             if (field.Primary)
                 sb.Append(Keywords.Blank)
                 .Append(Keywords.PrimaryKey);
         });
    }

    private static Keywords ConvertType(Type type)
    {
        if (type == SerializeTool.TByte ||
            type == SerializeTool.TChar ||
            type == SerializeTool.TShort ||
            type == SerializeTool.TInt ||
            type == SerializeTool.TLong)
            return Keywords.Integer;
        if (type == SerializeTool.TFloat ||
            type == SerializeTool.TDouble)
            return Keywords.Real;
        return Keywords.Text;
    }

    public static bool ConvertType(this SQLiteDataReader reader, Type type, out Func<int, object> convert)
    {
        convert = reader.GetString;
        if (type == SerializeTool.TByte)
            convert = i => reader.GetByte(i);
        else if (type == SerializeTool.TChar)
            convert = i => reader.GetChar(i);
        else if (type == SerializeTool.TShort)
            convert = i => reader.GetInt16(i);
        else if (type == SerializeTool.TInt)
            convert = i => reader.GetInt32(i);
        else if (type == SerializeTool.TLong)
            convert = i => reader.GetInt64(i);
        else if (type == SerializeTool.TFloat)
            convert = i => reader.GetFloat(i);
        else if (type == SerializeTool.TDouble)
            convert = i => reader.GetDouble(i);
        else if (type != SerializeTool.TString)
            return false;
        return true;
    }

    public static StringBuilder AppendConditions(this StringBuilder query, Conditions conditions, SignTable signTable)
    {
        if (conditions.Count < 1)
            return query;
        return query.Append(Keywords.Where)
            .AppendJoin(conditions.Combo.ToString(), conditions, (sb, condition) =>
            {
                var value = SerializeTool.Serialize(condition.Value, new(), false, signTable);
                sb.Append(condition.Key.ToQuoted())
                .Append(condition.Operate)
                .Append(value.ToQuoted());
            });
    }
}
