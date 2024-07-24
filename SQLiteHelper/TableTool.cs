using LocalUtilities.SQLiteHelper.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LocalUtilities.SQLiteHelper;

public class TableTool
{
    static BindingFlags Authority { get; } = BindingFlags.Public | BindingFlags.Instance;

    private static bool NotField(PropertyInfo property)
    {
        return property.GetCustomAttribute<TableIgnore>() is not null || property.SetMethod is null;
    }
    private static void GetFieldInfo(PropertyInfo property, out string name, out bool isPrimaryKey)
    {
        var fieldAtrribute = property.GetCustomAttribute<TableField>();
        name = fieldAtrribute?.Name ?? property.Name;
        isPrimaryKey = fieldAtrribute?.IsPrimaryKey ?? false;
    }

    public static Field[] GetFieldsName(Type type)
    {
        var fields = new List<Field>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldInfo(property, out var name, out var isPrimaryKey);
            fields.Add(new(name, property.PropertyType, isPrimaryKey));
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
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldInfo(property, out var name, out _);
            if (fields.TryGetValue(name, out var field))
                property.SetValue(obj, field.Value);
        }
        return obj;
    }

    public static Field[] GetFieldsValue(object obj)
    {
        var type = obj.GetType();
        var fields = new List<Field>();
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldInfo(property, out var name, out var isPrimaryKey);
            var value = property.GetValue(obj) ?? "";
            fields.Add(new(name, value, isPrimaryKey));
        }
        return fields.ToArray();
    }

    public static bool GetFieldName<T>(string? propertyName, [NotNullWhen(true)] out Field? field)
    {
        return GetFieldName(typeof(T), propertyName, out field);
    }

    public static bool GetFieldName(Type type, string? propertyName, [NotNullWhen(true)] out Field? field)
    {
        field = null;
        if (propertyName is not null)
        {
            var property = type.GetProperty(propertyName);
            if (property is null || NotField(property))
                return false;
            GetFieldInfo(property, out var name, out var isPrimaryKey);
            field = new(name, property.PropertyType, isPrimaryKey);
            return true;
        }
        foreach (var property in type.GetProperties(Authority))
        {
            if (NotField(property))
                continue;
            GetFieldInfo(property, out var name, out var isPrimaryKey);
            if (!isPrimaryKey)
                continue;
            field = new(name, property.PropertyType, isPrimaryKey);
            return true;
        }
        return false;
    }
}
