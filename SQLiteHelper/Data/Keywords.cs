namespace LocalUtilities.SQLiteHelper.Data;

public class Keywords
{
    public string Value { get; }

    private Keywords(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static Keywords Blank { get; } = new(" ");
    public static Keywords Any { get; } = new("*");
    public static Keywords Equal { get; } = new("=");
    public static Keywords Less { get; } = new("<");
    public static Keywords Greater { get; } = new(">");
    public static Keywords LessOrEqual { get; } = new("<=");
    public static Keywords GreaterOrEqual { get; } = new(">=");
    public static Keywords Quote { get; } = new("'");
    public static Keywords DoubleQuote { get; } = new("\"");
    public static Keywords Finish { get; } = new(";");
    public static Keywords Open { get; } = new("(");
    public static Keywords Close { get; } = new(")");
    public static Keywords Comma { get; } = new(",");
    public static Keywords DataSource { get; } = new("Data Source");
    public static Keywords Version { get; } = new(nameof(Version));
    public static Keywords Select { get; } = new(nameof(Select));
    public static Keywords From { get; } = new(nameof(From));
    public static Keywords InsertOrIgnoreInto { get; } = new("Insert Or Ignore Into");
    public static Keywords InsertOrReplaceInto { get; } = new("Insert Or Replace Into");
    public static Keywords Values { get; } = new(nameof(Values));
    public static Keywords Update { get; } = new(nameof(Update));
    public static Keywords Set { get; } = new(nameof(Set));
    public static Keywords Where { get; } = new(nameof(Where));
    public static Keywords Delete { get; } = new("Delete From");
    public static Keywords Or { get; } = new(nameof(Or));
    public static Keywords And { get; } = new(nameof(And));
    public static Keywords CreateTableIfNotExists { get; } = new("Create Table If Not Exists");
    public static Keywords WithoutRowid { get; } = new("Without Rowid");
    public static Keywords Text { get; } = new(nameof(Text));
    public static Keywords PrimaryKeyNotNull { get; } = new("Primary Key Not Null");
    public static Keywords Integer { get; } = new(nameof(Integer));
    public static Keywords Real { get; } = new(nameof(Real));
    public static Keywords SelectCount { get; } = new("Select Count");
    public static Keywords SqliteMaster { get; } = new("Sqlite_Master");
    public static Keywords Type { get; } = new("Type");
    public static Keywords Unique { get; } = new("Unique");
}
