using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper;

[AttributeUsage(AttributeTargets.Class)]
public class Table : Attribute
{

}

public class TableField : Attribute
{
    public string? Name { get; set; } = null;

    public bool IsPrimaryKey { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Property)]
public class TableIgnore : Attribute
{

}

