using System.Text;

namespace LocalUtilities.SQLiteHelper.Data;

public class Field(string name)
{
    public Volume Name { get; } = new(name);

    public override string ToString()
    {
        return new StringBuilder()
            .Append(Name)
            .Append(Keywords.Text)
            .ToString();
    }
}
