using LocalUtilities.SimpleScript;
using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.SQLiteHelper.Data;

public class Field(string name, object value, bool primary = false) : RosterItem<string>
{
    public override string Signature => Name;

    public string Name { get; } = name;

    public object Value { get; set; } = value;

    public bool Primary { get; } = primary;

    public Type Type { get; } = value.GetType();
}
