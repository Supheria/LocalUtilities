using LocalUtilities.TypeGeneral;

namespace LocalUtilities.SQLiteHelper.Data;

public class Field : IRosterItem<string>
{
    public string Signature => Name;

    public string Name { get; }

    public object? Value { get; set; }

    public bool IsPrimaryKey { get; }

    public Type Type { get; }

    public Field(string name, object value, bool isPrimaryKey = false)
    {
        Name = name;
        Value = value;
        IsPrimaryKey = isPrimaryKey;
        Type = value.GetType();
    }

    public Field(string name, Type type, bool isPrimaryKey = false)
    {
        Name = name;
        Value = null;
        Type = type;
        IsPrimaryKey = isPrimaryKey;
    }
}
