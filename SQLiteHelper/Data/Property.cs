using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

public class Property(string name, object? value) : IRosterItem<string>
{
    public string Name { get; } = name;

    public object? Value { get; } = value;

    public string Signature => Name;
}
