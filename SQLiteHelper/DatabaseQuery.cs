using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using LocalUtilities.TypeToolKit.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;
using static LocalUtilities.SQLiteHelper.Data.Conditions;

namespace LocalUtilities.SQLiteHelper;
/// <summary>
/// SQLite 操作类
/// </summary>
public class DatabaseQuery
{
    public static string Version { get; } = "3";

    static DatabaseSignTable SignTable { get; } = new();

    SQLiteConnection? Connection { get; set; }

    public void Connect(string filePath)
    {
        var query = new StringBuilder()
            .Append(Keywords.DataSource)
            .Append(Keywords.Equal)
            .Append(filePath.ToQuoted())
            .Append(Keywords.Finish)
            .Append(Keywords.Version)
            .Append(Keywords.Equal)
            .Append(Version.ToQuoted())
            .ToString();
        Connection = new(query);
        Connection.Open();
    }

    public void Close()
    {
        Connection?.Close();

    }

    private SQLiteDataReader? ExecuteQuery(string query)
    {
        if (Connection is null)
            return null;
        using var command = Connection.CreateCommand();
        command.CommandText = query;
        return command.ExecuteReader();
    }

    public void CreateTable(string name, params Field[] fields)
    {
        var query = new StringBuilder()
            .Append(Keywords.CreateTableNotExists)
            .Append(name.ToQuoted())
            .Append(Keywords.Open)
            .AppendFieldsName(fields)
            .Append(Keywords.Close)
            .Append(Keywords.WithoutRowid);
        ExecuteQuery(query.ToString());
    }

    public void InsertFieldsValue(string name, params Field[] fields)
    {
        var query = new StringBuilder()
             .Append(Keywords.InsertInto)
             .Append(name.ToQuoted())
             .Append(Keywords.Values)
             .Append(Keywords.Open)
            .AppendJoin(Keywords.Comma.ToString(), fields, (sb, field) =>
            {
                var value = SerializeTool.Serialize(field.Value, new(), false, SignTable);
                sb.Append(value.ToQuoted());
            })
            .Append(Keywords.Close);
        _ = ExecuteQuery(query.ToString());
    }

    /// <summary>
    /// 更新指定数据表内的数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    /// <param name="key">关键字</param>
    /// <param name="value">关键字对应的值</param>
    /// <param name="operation">运算符：=,<,>,...，默认“=”</param>
    public void UpdateFieldsValues(string name, Conditions conditions, params Field[] fields)
    {
        var query = new StringBuilder()
            .Append(Keywords.Update)
            .Append(name.ToQuoted())
            .Append(Keywords.Set)
            .AppendJoin(SignCollection.Comma, fields, (sb, field) =>
            {
                var value = SerializeTool.Serialize(field.Value, new(), false, SignTable);
                sb.Append(field.Name.ToQuoted())
                    .Append(Keywords.Equal)
                    .Append(value.ToQuoted());
            })
            .AppendConditions(conditions, SignTable);
        _ = ExecuteQuery(query.ToString());
    }

    public List<Fields> SelectFieldsValue(string name, Conditions conditions, params Field[] fields)
    {
        var query = new StringBuilder()
            .Append(Keywords.Select)
            .AppendJoin(Keywords.Comma.ToString(), fields, (sb, field) =>
            {
                sb.Append(field.Name.ToQuoted());
            })
            .Append(Keywords.From)
            .Append(name.ToQuoted())
            .AppendConditions(conditions, SignTable);
        using var reader = ExecuteQuery(query.ToString());
        if (reader is null)
            return [];
        var result = new List<Fields>();
        while (reader.Read())
        {
            var roster = new Fields();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                object? obj;
                var ordinal = reader.GetOrdinal(field.Name);
                if (reader.ConvertType(field.Type, out var convert))
                    obj = convert(ordinal);
                else
                {
                    var str = (string)convert(ordinal);
                    obj = SerializeTool.Deserialize(field.Type, new(), str, SignTable);
                }
                if (obj is null)
                    continue;
                roster.Add(new(field.Name, obj));
            }
            result.Add(roster);
        }
        return result;
    }

    /// <summary>
    /// 删除指定数据表内的数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    public void DeleteFields(string name, Conditions conditions)
    {
        var query = new StringBuilder()
            .Append(Keywords.Delete)
            .Append(name.ToQuoted())
            .AppendConditions(conditions, SignTable);
        _ = ExecuteQuery(query.ToString());
    }

    /// <summary>
    /// Reads the table.
    /// </summary>
    /// <returns>The table.</returns>
    /// <param name="tableName">Table name.</param>
    /// <param name="items">Items.</param>
    /// <param name="colNames">Col names.</param>
    /// <param name="operations">Operations.</param>
    /// <param name="colValues">Col values.</param>
    //public SQLiteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
    //{
    //    string queryString = "SELECT " + items[0];
    //    for (int i = 1; i < items.Length; i++)
    //    {
    //        queryString += ", " + items[i];
    //    }
    //    queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
    //    for (int i = 0; i < colNames.Length; i++)
    //    {
    //        queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
    //    }
    //    return ExecuteQuery(queryString);
    //}
}
