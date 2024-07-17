using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace LocalUtilities.SQLiteHelper;
/// <summary>
/// SQLite 操作类
/// </summary>
public class DatabaseOperation
{
    public static Volume Version { get; } = new("3");

    static string SubModulStampField { get; } = "sub-module stamp";

    static string DataName { get; } = "Data";

    DatabaseSignTable SignTable { get; } = new();
    /// <summary>
    /// 数据库连接定义
    /// </summary>
    SQLiteConnection? Connection { get; set; }

    public void Connect(string filePath)
    {
        var query = new QueryComposer()
            .Append(Keywords.DataSource)
            .Append(Keywords.Equal)
            .Append(new(filePath))
            .Finish()
            .Append(Keywords.Version)
            .Append(Keywords.Equal)
            .Append(Version)
            .Finish()
            .ToString();
        Connection = new(query);
        Connection.Open();
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void Close()
    {
        Connection?.Close();

    }

    /// <summary>
    /// 执行SQL命令
    /// </summary>
    /// <returns>The query.</returns>
    /// <param name="queryString">SQL命令字符串</param>
    private SQLiteDataReader? ExecuteQuery(QueryComposer queryComposer)
    {
        if (Connection is null)
            return null;
        using var command = Connection.CreateCommand();
        command.CommandText = queryComposer.ToString();
        return command.ExecuteReader();
    }

    public object[] ReadFullTable(Type type)
    {
        return ReadTable(type, null, null);
    }

    /// <summary>
    /// 读取整张数据表
    /// </summary>
    /// <returns>The full table.</returns>
    /// <param name="tableName">数据表名称</param>
    private object[] ReadTable(Type type, string? tableName, string? stamp)
    {
        var objects = new List<object>();
        var table = type.GetCustomAttribute<Table>();
        if (table is null)
            return objects.ToArray();
        if (tableName is null)
            tableName = table.Name ?? type.Name;
        else
            tableName = tableName + SignCollection.Dot + (table.Name ?? type.Name);
        var query = new QueryComposer()
            .Append(Keywords.Select)
            .Append(Keywords.Any)
            .Append(Keywords.From)
            .Append(new(tableName));
        if (stamp is not null)
            query.AppendCondition(new(new(SubModulStampField), new(stamp), Condition.Operates.Equal));
        query.Finish();
        using var reader = ExecuteQuery(query);
        if (reader is null)
            return [];
        while(reader.Read())
        {
            var obj = Activator.CreateInstance(type);
            if (obj is null)
                continue;
            foreach (var property in type.GetProperties())
            {
                if (property.GetCustomAttribute<TableFieldIgnore>() is not null || property.SetMethod is null)
                    continue;
                var subTable = property.PropertyType.GetCustomAttribute<Table>();
                if (subTable is not null)
                {
                    stamp = reader.GetString(reader.GetOrdinal(subTable.Name ?? property.Name));
                    var subObj = ReadTable(property.PropertyType, tableName, stamp);
                    if (subObj is not null)
                        property.SetValue(obj, subObj[0]);
                }
                else
                {
                    var ordinal = reader.GetOrdinal(property.GetCustomAttribute<TableField>()?.Name ?? property.Name);
                    var buffer = Encoding.UTF8.GetBytes(reader.GetString(ordinal));
                    var subObj = SerializeTool.Deserialize(property.PropertyType, buffer, 0, buffer.Length, DataName, SignTable);
                    if (subObj is not null)
                        property.SetValue(obj,subObj);
                }
            }
            objects.Add(obj);
        }
        return objects.ToArray();
    }

    public void CreateTable(Type type)
    {
        CreateTable(type, null);
    }

    private void CreateTable(Type type, string? tableName)
    {
        var table = type.GetCustomAttribute<Table>();
        if (table is null)
            return;
        var fields = new List<Field>();
        if (tableName is null)
            tableName = table.Name ?? type.Name;
        else
        {
            tableName = tableName + SignCollection.Dot + (table.Name ?? type.Name);
            fields.Add(new(SubModulStampField));
        }
        foreach (var property in type.GetProperties())
        {
            if (property.GetCustomAttribute<TableFieldIgnore>() is not null || property.SetMethod is null)
                continue;
            var subTable = property.PropertyType.GetCustomAttribute<Table>();
            if (subTable is not null)
            {
                CreateTable(property.PropertyType, tableName);
                fields.Add(new(subTable.Name ?? property.Name));
                continue;
            }
            var tableField = property.GetCustomAttribute<TableField>();
            fields.Add(new(tableField?.Name ?? property.Name));
        }
        var query = new QueryComposer()
            .Append(Keywords.CreateTableNotExists)
            .Append(new(tableName))
            .AppendFields(fields.ToArray())
            .Finish();
        _ = ExecuteQuery(query);
    }

    public void InsertFields(object obj)
    {
        InsertFields(obj, null, null);
    }

    private void InsertFields(object? obj, string? tableName, Volume? stamp)
    {
        if (obj is null)
            return;
        var type = obj.GetType();
        var table = type.GetCustomAttribute<Table>();
        if (table is null)
            return;
        if (tableName is null)
            tableName = table.Name ?? type.Name;
        else
            tableName = tableName + SignCollection.Dot + (table.Name ?? type.Name);
        var fieldValues = new List<Volume>();
        if (stamp is not null)
            fieldValues.Add(stamp);
        stamp = GetStamp();
        foreach (var property in type.GetProperties())
        {
            if (property.GetCustomAttribute<TableFieldIgnore>() is not null || property.SetMethod is null)
                continue;
            var subObj = property.GetValue(obj);
            if (property.PropertyType.GetCustomAttribute<Table>() is not null)
            {
                InsertFields(subObj, tableName, stamp);
                fieldValues.Add(stamp);
                continue;
            }
            var buffer = SerializeTool.Serialize(subObj, DataName, SignTable) ?? [];
            fieldValues.Add(new(Encoding.UTF8.GetString(buffer)));
        }
        var query = new QueryComposer()
             .Append(Keywords.InsertInto)
             .Append(new(tableName))
             .AppendValues(fieldValues.ToArray())
             .Finish();
        _ = ExecuteQuery(query);
    }

    private static Volume GetStamp()
    {
        return new(DateTime.Now.ToBinary().ToString());
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
    public SQLiteDataReader UpdateValues(Volume tableName, Condition condition, params Assignment[] updateFields)
    {
        var query = new QueryComposer()
            .Append(Keywords.Update)
            .Append(tableName)
            .Append(Keywords.Set)
            .AppendColumnFields(updateFields)
            .AppendCondition(condition)
            .Finish();
        return ExecuteQuery(query);
    }

    /// <summary>
    /// 删除指定数据表内的数据
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="tableName">数据表名称</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colValues">字段名对应的数据</param>
    public SQLiteDataReader DeleteValues(Volume tableName, Condition[] conditions, Condition.Combos combo)
    {
        var query = new QueryComposer()
            .Append(Keywords.Delete)
            .Append(Keywords.From)
            .Append(tableName)
            .AppendConditions(conditions, combo)
            .Finish();
        return ExecuteQuery(query);
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