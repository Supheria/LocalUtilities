using LocalUtilities.SimpleScript;
using LocalUtilities.SQLiteHelper.Data;
using LocalUtilities.TypeGeneral;
using LocalUtilities.TypeToolKit;
using LocalUtilities.TypeToolKit.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper;

partial class SQLiteQuery
{
    static BindingFlags Authority { get; } = BindingFlags.Public | BindingFlags.Instance;

    public static string QuoteName(string name)
    {
        return new StringBuilder()
            .Append(Keywords.DoubleQuote)
            .Append(name)
            .Append(Keywords.DoubleQuote)
            .ToString();
    }

    public static string QuoteValue(string value)
    {
        return new StringBuilder()
            .Append(Keywords.Quote)
            .Append(value)
            .Append(Keywords.Quote)
            .ToString();
    }

    public static string GetConditionsString(Condition?[] conditions, ConditionCombo combo)
    {
        if (conditions.Length < 1)
            return "";
        var comboWord = combo switch
        {
            ConditionCombo.Or => Keywords.Or,
            ConditionCombo.And => Keywords.And,
            _ => Keywords.Or
        };
        var sb = new StringBuilder();
        var first = true;
        foreach (var condition in conditions)
        {
            if (condition is null)
                continue;
            if (first)
            {
                sb.Append(Keywords.Where);
                first = false;
            }
            else
                sb.Append(comboWord);
            var value = SerializeTool.Serialize(condition.Value, new(), SignTable, false);
            sb.Append(QuoteName(condition.FieldName))
                .Append(condition.Operate)
                .Append(QuoteValue(value));
        };
        return sb.ToString();
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

    public static Func<int, string> ConvertType(SQLiteDataReader reader, Type type)
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

    private static bool NotField(PropertyInfo property)
    {
        return property.GetCustomAttribute<TableIgnore>() is not null || property.SetMethod is null;
    }

    private static void GetFieldNameInfo(PropertyInfo property, out string name, out bool isPrimaryKey)
    {
        var fieldAtrribute = property.GetCustomAttribute<TableField>();
        name = fieldAtrribute?.Name ?? property.Name;
        if (name.Contains(Keywords.Quote.ToString()) || name.Contains(Keywords.DoubleQuote.ToString()))
            name = property.Name;
        isPrimaryKey = fieldAtrribute?.IsPrimaryKey ?? false;
    }

    /// <summary>
    /// get value from <typeparamref name="T"/>'s field of given <paramref name="propertyName"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static FieldName? GetFieldName<T>(string propertyName)
    {
        var type = typeof(T);
        var property = type.GetProperty(propertyName);
        if (property is null || NotField(property))
            return null;
        GetFieldNameInfo(property, out var name, out var isPrimaryKey);
        return new(name, property, isPrimaryKey);
    }

    /// <summary>
    /// get value from <typeparamref name="T"/>'s field which is primary key 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static FieldName? GetFieldName<T>()
    {
        var type = typeof(T);
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            if (isPrimaryKey)
                return new(name, property, isPrimaryKey);
        }
        return null;
    }

    /// <summary>
    /// get names from all <typeparamref name="T"/>'s fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static FieldName[] GetFieldNames<T>()
    {
        var type = typeof(T);
        var fieldNames = new List<FieldName>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            fieldNames.Add(new(name, property, isPrimaryKey));
        }
        return fieldNames.ToArray();
    }

    /// <summary>
    /// get names from <typeparamref name="T"/>'s fields of given <paramref name="propertyNames"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    public static FieldName[] GetFieldNames<T>(params string[] propertyNames)
    {
        var type = typeof(T);
        var fieldNames = new List<FieldName>();
        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName);
            if (property is null || NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            fieldNames.Add(new(name, property, isPrimaryKey));
        }
        return fieldNames.ToArray();
    }

    /// <summary>
    /// get value from <paramref name="obj"/>'s field of given <paramref name="propertyName"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static FieldValue? GetFieldValue(object obj, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName);
        if (property is null || NotField(property))
            return null;
        GetFieldNameInfo(property, out var name, out var isPrimaryKey); 
        return new(name, property.GetValue(obj), isPrimaryKey);
    }

    /// <summary>
    /// get value from <paramref name="obj"/>'s field which is primary key 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static FieldValue? GetFieldValue(object obj)
    {
        var type = obj.GetType();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            if (isPrimaryKey)
                return new(name, property.GetValue(obj), isPrimaryKey);
        }
        return null;
    }

    /// <summary>
    /// get values from all <paramref name="obj"/>'s fields
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static FieldValue[] GetFieldValues(object obj)
    {
        var type = obj.GetType();
        var fieldValues = new List<FieldValue>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            fieldValues.Add(new(name, property.GetValue(obj), isPrimaryKey));
        }
        return fieldValues.ToArray();
    }

    /// <summary>
    /// get values from <paramref name="obj"/>'s fields of given <paramref name="propertyNames"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    public static FieldValue[] GetFieldValues(object obj, params string[] propertyNames)
    {
        var type = obj.GetType();
        var fieldValues = new List<FieldValue>();
        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName);
            if (property is null || NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            fieldValues.Add(new(name, property.GetValue(obj), isPrimaryKey));
        }
        return fieldValues.ToArray();
    }

    /// <summary>
    /// get <see cref="Condition"/> from <paramref name="obj"/>'s field of given <paramref name="propertyName"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="operate"></param>
    /// <returns></returns>
    public static Condition? GetCondition(object obj, Operators operate, string propertyName)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyName);
        if (property is null || NotField(property))
            return null;
        GetFieldNameInfo(property, out var name, out _);
        return new(name, property.Name, property.GetValue(obj), operate);
    }

    /// <summary>
    /// get <see cref="Condition"/> from <paramref name="obj"/>'s field which is primary key
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="operate"></param>
    /// <returns></returns>
    public static Condition? GetCondition(object obj, Operators operate)
    {
        var type = obj.GetType();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out var isPrimaryKey);
            if (isPrimaryKey)
                return new(name, property.Name, property.GetValue(obj), operate);
        }
        return null;
    }

    /// <summary>
    /// get <see cref="Operators.Equal"/> <see cref="Condition"/>s from all <paramref name="obj"/>'s fields
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>roster of <see cref="Operators.Equal"/> <see cref="Condition"/></returns>
    public static ConditionRoster GetConditions(object obj)
    {
        var type = obj.GetType();
        var conditions = new ConditionRoster();
        foreach (var property in type.GetProperties())
        {
            if (NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out _);
            conditions.TryAdd(new(name, property.Name, property.GetValue(obj), Operators.Equal));
        }
        return conditions;
    }

    /// <summary>
    /// get many <see cref="Operators.Equal"/> <see cref="Condition"/>s from <paramref name="obj"/>'s fields of given <paramref name="propertyNames"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyNames"></param>
    /// <returns>roster of <see cref="Operators.Equal"/> <see cref="Condition"/></returns>
    public static ConditionRoster GetConditions(object obj, params string[] propertyNames)
    {
        var type = obj.GetType();
        var conditions = new ConditionRoster();
        foreach (var propertyName in propertyNames)
        {
            var property = type.GetProperty(propertyName);
            if (property is null || NotField(property))
                continue;
            GetFieldNameInfo(property, out var name, out _);
            conditions.TryAdd(new(name, property.Name, property.GetValue(obj), Operators.Equal));
        }
        return conditions;
    }
}
