using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Data.SQLite;
using System.Text;

namespace LocalUtilities.SQLiteHelper;

public class SQLiteQuery : IDisposable
{
    public static string Version { get; } = "3";

    static DatabaseSignTable SignTable { get; } = new();

    SQLiteConnection Connection { get; set; }

    public SQLiteQuery(string filePath)
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

    public void Dispose()
    {
        Connection.Close();
        GC.SuppressFinalize(this);
    }

    private SQLiteDataReader? ExecuteQuery(string query)
    {
        if (Connection is null)
            return null;
        using var command = Connection.CreateCommand();
        command.CommandText = query;
        return command.ExecuteReader();
    }

    public void CreateTable(string name, Field[] fields)
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

    public void InsertFieldsValue(string name, Field[] fields)
    {
        var query = new StringBuilder()
             .Append(Keywords.InsertInto)
             .Append(name.ToQuoted())
             .Append(Keywords.Values)
             .Append(Keywords.Open)
            .AppendJoin(Keywords.Comma.ToString(), fields, (sb, field) =>
            {
                var value = SerializeTool.Serialize(field.Value, new(), SignTable, false);
                sb.Append(value.ToQuoted());
            })
            .Append(Keywords.Close);
        _ = ExecuteQuery(query.ToString());
    }

    public void UpdateFieldsValues(string name, Field[] fields, Condition? condition)
    {
        UpdateFieldsValues(name, fields, condition is null ? [] : [condition], Condition.Combo.Default);
    }

    public void UpdateFieldsValues(string name, Field[] fields, Condition[] conditions, Condition.Combo combo)
    {
        var query = new StringBuilder()
           .Append(Keywords.Update)
           .Append(name.ToQuoted())
           .Append(Keywords.Set)
           .AppendJoin(SignCollection.Comma, fields, (sb, field) =>
           {
               var value = SerializeTool.Serialize(field.Value, new(), SignTable, false);
               sb.Append(field.Name.ToQuoted())
                   .Append(Keywords.Equal)
                   .Append(value.ToQuoted());
           })
           .AppendConditions(conditions, combo, SignTable);
        _ = ExecuteQuery(query.ToString());
    }

    public List<Fields> SelectFieldsValue(string name, Field[] fields, Condition? condition)
    {
        return SelectFieldsValue(name, fields, condition is null ? [] : [condition], Condition.Combo.Default);
    }

    public List<Fields> SelectFieldsValue(string name, Field[] fields, Condition[] conditions, Condition.Combo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.Select)
            .AppendJoin(Keywords.Comma.ToString(), fields, (sb, field) =>
            {
                sb.Append(field.Name.ToQuoted());
            })
            .Append(Keywords.From)
            .Append(name.ToQuoted())
            .AppendConditions(conditions, combo, SignTable);
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
                var convert = reader.ConvertType(field.Type);
                var str = convert(reader.GetOrdinal(field.Name));
                obj = SerializeTool.Deserialize(field.Type, new(), str, SignTable);
                if (obj is not null)
                    roster.TryAdd(new(field.Name, obj));
            }
            result.Add(roster);
        }
        return result;
    }

    public void DeleteFields(string name, Condition? condition)
    {
        DeleteFields(name, condition is null ? [] : [condition], Condition.Combo.Default);
    }

    public void DeleteFields(string name, Condition[] conditions, Condition.Combo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.Delete)
            .Append(name.ToQuoted())
            .AppendConditions(conditions, combo, SignTable);
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
