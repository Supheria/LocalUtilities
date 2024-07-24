using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit.Text;
using System.Data.SQLite;
using System.Text;
using System.Transactions;

namespace LocalUtilities.SQLiteHelper;

public class SQLiteQuery : IDisposable
{
    public static string Version { get; } = "3";

    static DatabaseSignTable SignTable { get; } = new();

    SQLiteConnection Connection { get; }

    SQLiteCommand Command { get; }

    SQLiteTransaction Transaction { get; }

    public SQLiteQuery(string filePath)
    {
        var query = new StringBuilder()
            .Append(Keywords.DataSource)
            .Append(Keywords.Equal)
            .Append(filePath.ToQuoted());
        Connection = new(query.ToString());
        Connection.Open();
        Command = Connection.CreateCommand();
        Transaction = Connection.BeginTransaction();
    }

    public void Dispose()
    {
        Transaction.Commit();
        Command.Dispose();
        Connection.Close();
        GC.SuppressFinalize(this);
    }

    private void ExecuteNonQuery(string query)
    {
        try
        {
            Command.CommandText = query;
            Command.ExecuteNonQuery();
        }
        catch
        {
            Transaction.Rollback();
            throw;
        }
    }

    private SQLiteDataReader ExecuteReader(string query)
    {
        try
        {
            Command.CommandText = query;
            return Command.ExecuteReader();
        }
        catch
        {
            Transaction.Rollback();
            throw;
        }
    }

    private object ExecuteScalar(string query)
    {
        try
        {
            Command.CommandText = query;
            return Command.ExecuteScalar();
        }
        catch
        {
            Transaction.Rollback();
            throw;
        }
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
        ExecuteNonQuery(query.ToString());
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
        ExecuteNonQuery(query.ToString());
    }

    public void InsertManyFieldsValue(string name, List<Field[]> fields)
    {

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
        ExecuteNonQuery(query.ToString());
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
        using var reader = ExecuteReader(query.ToString());
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
        ExecuteNonQuery(query.ToString());
    }

    public int Sum(string name, Field? field, Condition? condition)
    {
        return Sum(name, field, condition is null ? [] : [condition], Condition.Combo.Default);
    }

    public int Sum(string name, Field? field, Condition[] conditions, Condition.Combo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.SelectCount)
            .Append(Keywords.Open);
        if (field is null)
            query.Append(Keywords.Any);
        else
            query.Append(field.Name.ToQuoted());
        query.Append(Keywords.Close)
            .Append(Keywords.From)
            .Append(name.ToQuoted())
            .AppendConditions(conditions, combo, SignTable);
        return Convert.ToInt32(ExecuteScalar(query.ToString()));
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
