using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper;

public abstract class TableAttribute : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class Table : TableAttribute
{

}

[AttributeUsage(AttributeTargets.Property)]
public class TableField : TableAttribute
{

}

[AttributeUsage(AttributeTargets.Property)]
public class TableFieldIgnore : Attribute
{

}
