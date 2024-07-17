using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

public class Keywords
{
    public string Value { get; }

    private Keywords(string value)
    {
        Value = value + SignCollection.Space;
    }

    private Keywords() : this("")
    {

    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator Volume(Keywords keyWord)
    {
        return new(keyWord);
    }

    public static Keywords Null { get; } = new();
    public static Keywords Any { get; } = new(SignCollection.Asterisk.ToString());
    public static Keywords Equal { get; } = new("=");
    public static Keywords Less { get; } = new("<");
    public static Keywords Greater { get; } = new(">");
    public static Keywords DataSource { get; } = new("Data Source");
    public static Keywords Version { get; } = new(nameof(Version));
    public static Keywords Select { get; } = new(nameof(Select));
    public static Keywords From { get; } = new(nameof(From));
    public static Keywords InsertInto { get; } = new("Insert Into");
    public static Keywords Values { get; } = new(nameof(Values));
    public static Keywords Update { get; } = new(nameof(Update));
    public static Keywords Set { get; } = new(nameof(Set));
    public static Keywords Where { get; } = new(nameof(Where));
    public static Keywords Delete { get; } = new(nameof(Delete));
    public static Keywords Or { get; } = new(nameof(Or));
    public static Keywords And { get; } = new(nameof(And));
    public static Keywords CreateTableNotExists { get; } = new("Create Table If Not Exists");
    public static Keywords Text { get; } = new(nameof(Text));
    public static Keywords Integer { get; } = new(nameof(Integer));
    public static Keywords Real { get; } = new(nameof(Real));
}
