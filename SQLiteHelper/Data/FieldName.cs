using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

public class FieldName(string name, string properyName, Type type, bool isPrimaryKey)
{
    public string Name { get; } = name;

    public string PropertyName { get; } = properyName;

    public Type Type { get; } = type;

    public bool IsPrimaryKey { get; set; } = isPrimaryKey;

    public PropertyInfo? Property { get; } = null;

    public FieldName(string name, PropertyInfo property, bool isPrimaryKey) : this(name, property.Name, property.PropertyType, isPrimaryKey)
    {
        Property = property;
    }
}
