namespace LocalUtilities.SQLiteHelper;

[AttributeUsage(AttributeTargets.Property)]
public class TableField : Attribute
{
    public string? Name { get; set; } = null;

    public bool IsPrimaryKey { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Property)]
public class TableIgnore : Attribute
{

}

