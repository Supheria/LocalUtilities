﻿using LocalUtilities.General;
using LocalUtilities.SimpleScript;
using System.Data.SQLite;
using System.Text;

namespace LocalUtilities.SQLiteHelper;

public partial class SQLiteQuery : IDisposable
{
    SQLiteConnection Connection { get; }

    SQLiteCommand Command { get; }

    SQLiteTransaction? Transaction { get; set; }

    /// <summary>
    /// a <see cref="SQLiteTransaction"/> begins, use <see cref="Commit"/> or <see cref="Dispose"/> to commit it
    /// </summary>
    /// <param name="filePath"></param>
    public SQLiteQuery(string filePath)
    {
        var query = new StringBuilder()
            .Append(Keywords.DataSource)
            .Append(Keywords.Equal)
            .Append(QuoteValue(filePath));
        Connection = new(query.ToString());
        Connection.Open();
        Command = Connection.CreateCommand();
    }

    public void Dispose()
    {
        Transaction?.Commit();
        Transaction?.Dispose();
        Command.Dispose();
        Connection.Close();
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Begin()
    {
        Transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        Transaction?.Commit();
        Transaction?.Dispose();
        Transaction = null;
    }

    private void ExecuteNonQuery(string query)
    {
        Command.CommandText = query;
        Command.ExecuteNonQuery();
    }

    private SQLiteDataReader ExecuteReader(string query)
    {
        Command.CommandText = query;
        return Command.ExecuteReader();
    }

    private object ExecuteScalar(string query)
    {
        Command.CommandText = query;
        return Command.ExecuteScalar();
    }

    public List<string> ListAllTableNames()
    {
        var query = new StringBuilder()
            .Append(Keywords.Select)
            .Append(QuoteName("name"))
            .Append(Keywords.From)
            .Append(QuoteName("sqlite_master"))
            .Append(Keywords.Where)
            .Append(QuoteName("type"))
            .Append(Keywords.Equal)
            .Append(QuoteValue("table"));
        using var reader = ExecuteReader(query.ToString());
        var names = new List<string>();
        while (reader.Read())
            names.Add((string)reader["name"]);
        return names;
    }

    public void CreateTable(string tableName, FieldName[] fieldNames)
    {
        var hasPrimaryKey = false;
        var query = new StringBuilder()
            .Append(Keywords.CreateTableIfNotExists)
            .Append(QuoteName(tableName))
            .Append(Keywords.Open)
            .AppendJoin(Keywords.Comma.ToString(), fieldNames, (sb, field) =>
            {
                sb.Append(QuoteName(field.Name))
                .Append(ConvertType(field.Type));
                if (field.IsPrimaryKey)
                {
                    sb.Append(Keywords.Blank)
                    .Append(Keywords.PrimaryKeyNotNull);
                    hasPrimaryKey = true;
                }
            })
            .Append(Keywords.Close);
        if (hasPrimaryKey)
            query.Append(Keywords.WithoutRowid);
        ExecuteNonQuery(query.ToString());
    }

    public void CreateTable<T>(string tableName)
    {
        var type = typeof(T);
        var query = new StringBuilder()
            .Append(Keywords.CreateTableIfNotExists)
            .Append(QuoteName(tableName))
            .Append(Keywords.Open);
        var first = true;
        var hasPrimaryKey = false;
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey, out var isUnique);
            if (!first)
                query.Append(Keywords.Comma);
            else
                first = false;
            query.Append(QuoteName(name))
                .Append(ConvertType(property.PropertyType));
            if (isPrimaryKey)
            {
                query.Append(Keywords.Blank)
                    .Append(Keywords.PrimaryKeyNotNull);
                hasPrimaryKey = true;
            }
            else if (isUnique)
                query.Append(Keywords.Blank)
                    .Append(Keywords.Unique);
        }
        query.Append(Keywords.Close);
        if (hasPrimaryKey)
            query.Append(Keywords.WithoutRowid);
        ExecuteNonQuery(query.ToString());
    }

    public void InsertItem(string tableName, FieldValue[] fieldValues, InsertTypes insertType)
    {
        var query = new StringBuilder();
        if (insertType is InsertTypes.IgnoreIfExists)
            query.Append(Keywords.InsertOrIgnoreInto);
        else
            query.Append(Keywords.InsertOrReplaceInto);
        query.Append(QuoteName(tableName))
            .Append(Keywords.Values)
            .Append(Keywords.Open)
            .AppendJoin(Keywords.Comma.ToString(), fieldValues, (sb, value) =>
            {
                var val = SerializeTool.Serialize(value.Value, false);
                sb.Append(QuoteValue(val));
            })
            .Append(Keywords.Close);
        ExecuteNonQuery(query.ToString());
    }

    public void InsertItem(string tableName, object obj, InsertTypes insertType)
    {
        var type = obj.GetType();
        var query = new StringBuilder();
        if (insertType is InsertTypes.IgnoreIfExists)
            query.Append(Keywords.InsertOrIgnoreInto);
        else
            query.Append(Keywords.InsertOrReplaceInto);
        query.Append(QuoteName(tableName))
            .Append(Keywords.Values)
            .Append(Keywords.Open);
        var first = true;
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            if (!first)
                query.Append(Keywords.Comma);
            else
                first = false;
            var value = SerializeTool.Serialize(property.GetValue(obj), false);
            query.Append(QuoteValue(value));
        }
        query.Append(Keywords.Close);
        ExecuteNonQuery(query.ToString());
    }

    public void InsertItems<T>(string tableName, T[] objs, InsertTypes insertType)
    {
        var type = typeof(T);
        var valueTable = new StringBuilder[objs.Length];
        var first = true;
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            for (var i = 0; i < objs.Length; i++)
            {
                if (!first)
                    valueTable[i].Append(Keywords.Comma);
                else
                    valueTable[i] = new();
                var value = SerializeTool.Serialize(property.GetValue(objs[i]), false);
                valueTable[i].Append(QuoteValue(value));
            }
            first = false;
        }
        var query = new StringBuilder();
        if (insertType is InsertTypes.IgnoreIfExists)
            query.Append(Keywords.InsertOrIgnoreInto);
        else
            query.Append(Keywords.InsertOrReplaceInto);
        query.Append(QuoteName(tableName))
            .Append(Keywords.Values)
            .AppendJoin(Keywords.Comma.ToString(), valueTable, (sb, valueString) =>
            {
                sb.Append(Keywords.Open)
                .Append(valueString)
                .Append(Keywords.Close);
            });
        ExecuteNonQuery(query.ToString());
    }

    /// <summary>
    /// update an item completely fits to <paramref name="obj"/>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="obj"></param>
    public void UpdateItem(string tableName, object obj)
    {
        var type = obj.GetType();
        var query = new StringBuilder()
           .Append(Keywords.Update)
           .Append(QuoteName(tableName))
           .Append(Keywords.Set);
        var first = true;
        Condition? condition = null;
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey, out _);
            var value = SerializeTool.Serialize(property.GetValue(obj), false);
            if (isPrimaryKey)
            {
                condition = new(name, value, Operators.Equal);
                continue;
            }
            if (!first)
                query.Append(Keywords.Comma);
            else
                first = false;
            query.Append(QuoteName(name))
                .Append(Keywords.Equal)
                .Append(QuoteValue(value));
        }
        query.Append(GetConditionsString([condition], ConditionCombo.Default));
        ExecuteNonQuery(query.ToString());
    }

    /// <summary>
    /// <para>field which is primary key won't be updated</para>
    /// <para>set <paramref name="condition"/> null for no condition limit</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldValues"></param>
    /// <param name="condition"></param>
    public void UpdateItems(string tableName, FieldValue[] fieldValues, Condition? condition)
    {
        UpdateItems(tableName, fieldValues, [condition], ConditionCombo.Default);
    }

    /// <summary>
    /// <para>field which is primary key won't be updated</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldValues"></param>
    /// <param name="conditions"></param>
    /// <param name="combo"></param>
    public void UpdateItems(string tableName, FieldValue[] fieldValues, Condition?[] conditions, ConditionCombo combo)
    {
        var query = new StringBuilder()
           .Append(Keywords.Update)
           .Append(QuoteName(tableName))
           .Append(Keywords.Set);
        var first = true;
        foreach (var field in fieldValues)
        {
            if (field.IsPrimaryKey)
                continue;
            var value = SerializeTool.Serialize(field.Value, false);
            if (!first)
                query.Append(Keywords.Comma);
            else
                first = false;
            query.Append(QuoteName(field.Name))
                   .Append(Keywords.Equal)
                   .Append(QuoteValue(value));
        };
        query.Append(GetConditionsString(conditions, combo));
        ExecuteNonQuery(query.ToString());
    }

    /// <summary>
    /// <para>get properties from items of given fieldNames</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldNames">if empty will return empty array</param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public PropertyRoster[] SelectItems(string tableName, FieldName?[] fieldNames, Condition? condition)
    {
        return SelectItems(tableName, fieldNames, [condition], ConditionCombo.Default);
    }

    /// <summary>
    /// <para>get properties from items of given fieldNames</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldNames">if empty will return empty array</param>
    /// <param name="conditions"></param>
    /// <param name="combo"></param>
    /// <returns></returns>
    public PropertyRoster[] SelectItems(string tableName, FieldName?[] fieldNames, Condition?[] conditions, ConditionCombo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.Select);
        if (fieldNames is null || fieldNames.Length is 0)
            return [];
        var fieldNameList = new List<FieldName>();
        var first = true;
        foreach (var fieldName in fieldNames)
        {
            if (fieldName is null)
                continue;
            fieldNameList.Add(fieldName);
            if (!first)
                query.Append(Keywords.Comma);
            else
                first = false;
            query.Append(QuoteName(fieldName.Name));
        }
        query.Append(Keywords.From)
            .Append(QuoteName(tableName))
            .Append(GetConditionsString(conditions, combo));
        using var reader = ExecuteReader(query.ToString());
        var objs = new List<PropertyRoster>();
        while (reader.Read())
        {
            var properties = new PropertyRoster();
            foreach (var fieldName in fieldNameList)
            {
                var convert = ConvertType(reader, fieldName.Type);
                var str = convert(reader.GetOrdinal(fieldName.Name));
                var value = SerializeTool.Deserialize(fieldName.Type, str);
                properties.TryAdd(new(fieldName.PropertyName, value));
            }
            objs.Add(properties);
        }
        return objs.ToArray();
    }

    /// <summary>
    /// <para>get instances of <typeparamref name="T"/> from items</para>
    /// <para>set <paramref name="condition"/> null for no condition limit</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="fieldNames"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public T[] SelectItems<T>(string tableName, Condition? condition)
    {
        return SelectItems<T>(tableName, [condition], ConditionCombo.Default);
    }

    /// <summary>
    /// <para>get instances of <typeparamref name="T"/> from items</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="fieldNames"></param>
    /// <param name="conditions"></param>
    /// <param name="combo"></param>
    /// <returns></returns>
    public T[] SelectItems<T>(string tableName, Condition?[] conditions, ConditionCombo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.Select)
            .Append(Keywords.Any)
            .Append(Keywords.From)
            .Append(QuoteName(tableName))
            .Append(GetConditionsString(conditions, combo));
        using var reader = ExecuteReader(query.ToString());
        var type = typeof(T);
        var objs = new List<T>();
        var fieldNames = GetFieldNames<T>();
        while (reader.Read())
        {
            var obj = Activator.CreateInstance(type);
            if (obj is null)
                continue;
            foreach (var fieldName in fieldNames)
            {
                var convert = ConvertType(reader, fieldName.Type);
                var str = convert(reader.GetOrdinal(fieldName.Name));
                var subObj = SerializeTool.Deserialize(fieldName.Type, str);
                var property = fieldName.Property ?? type.GetProperty(fieldName.PropertyName);
                property?.SetValue(obj, subObj);
            }
            objs.Add((T)obj);
        }
        return objs.ToArray();
    }

    /// <summary>
    /// <para>set <paramref name="condition"/> null for no condition limit</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="condition"></param>
    public void DeleteItems(string tableName, Condition? condition)
    {
        DeleteItems(tableName, [condition], ConditionCombo.Default);
    }

    public void DeleteItems(string tableName, Condition?[] conditions, ConditionCombo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.Delete)
            .Append(QuoteName(tableName))
           .Append(GetConditionsString(conditions, combo));
        ExecuteNonQuery(query.ToString());
    }

    /// <summary>
    /// <para>set <paramref name="fieldName"/> null to select any</para>
    /// <para>set <paramref name="condition"/> null for no condition limit</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldName"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public int Sum(string tableName, FieldName? fieldName, Condition? condition)
    {
        return Sum(tableName, fieldName, [condition], ConditionCombo.Default);
    }

    /// <summary>
    /// <para>set <paramref name="fieldName"/> null to select any</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="fieldName"></param>
    /// <param name="conditions"></param>
    /// <param name="combo"></param>
    /// <returns></returns>
    public int Sum(string tableName, FieldName? fieldName, Condition?[] conditions, ConditionCombo combo)
    {
        var query = new StringBuilder()
            .Append(Keywords.SelectCount)
            .Append(Keywords.Open);
        if (fieldName is null)
            query.Append(Keywords.Any);
        else
            query.Append(QuoteName(fieldName.Name));
        query.Append(Keywords.Close)
            .Append(Keywords.From)
            .Append(QuoteName(tableName))
            .Append(GetConditionsString(conditions, combo));
        return Convert.ToInt32(ExecuteScalar(query.ToString()));
    }

    public bool Exist(string tableName, object obj)
    {
        var conditon = GetCondition(obj, Operators.Equal);
        var query = new StringBuilder()
            .Append(Keywords.SelectCount)
            .Append(Keywords.Open)
            .Append(Keywords.Any)
            .Append(Keywords.Close)
            .Append(Keywords.From)
            .Append(QuoteName(tableName))
            .Append(GetConditionsString([conditon], ConditionCombo.Default));
        return Convert.ToInt32(ExecuteScalar(query.ToString())) is not 0;
    }
}
