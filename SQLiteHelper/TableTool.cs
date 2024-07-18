using LocalUtilities.SQLiteHelper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper;

public class TableTool
{
    static BindingFlags Authority { get; } = BindingFlags.Public | BindingFlags.Instance;

    public static Field[] GetFieldsName(Type type)
    {
        if (type.GetCustomAttribute<Table>() is null)
            return [];
        var fields = new List<Field>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (property.GetCustomAttribute<TableIgnore>() is not null || property.SetMethod is null) 
                continue;
            var fieldAtrribute = property.GetCustomAttribute<TableField>();
            var name = fieldAtrribute?.Name ?? property.Name;
            fields.Add(new(name, property.PropertyType, fieldAtrribute?.IsPrimaryKey ?? false));
        }
        return fields.ToArray();
    }

    public static Field[] GetFieldsName<T>()
    {
        return GetFieldsName(typeof(T));
    }

    public static T SetFieldsValue<T>(T obj, Fields fields)
    {
        if (obj is null)
            return obj;
        var type = obj.GetType();
        if (type.GetCustomAttribute<Table>() is null)
            return obj;
        foreach (var property in type.GetProperties(Authority))
        {
            if (property.GetCustomAttribute<TableIgnore>() is not null || property.SetMethod is null)
                continue;
            var fieldAtrribute = property.GetCustomAttribute<TableField>();
            var name = fieldAtrribute?.Name ?? property.Name;
            if (!fields.TryGetValue(name, out var field))
                continue;
            property.SetValue(obj, field.Value);
        }
        return obj;
    }

    public static Field[] GetFieldsValue(object obj)
    {
        var type = obj.GetType();
        if (type.GetCustomAttribute<Table>() is null)
            return [];
        var fields = new List<Field>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (property.GetCustomAttribute<TableIgnore>() is not null || property.SetMethod is null)
                continue;
            var fieldAtrribute = property.GetCustomAttribute<TableField>();
            var name = fieldAtrribute?.Name ?? property.Name;
            var value = property.GetValue(obj) ?? "";
            fields.Add(new(name, value, fieldAtrribute?.IsPrimaryKey ?? false));
        }
        return fields.ToArray();
    }
}
