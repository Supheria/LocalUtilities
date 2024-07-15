using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Common;

[AttributeUsage(AttributeTargets.Class)]
public class SsRoot : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class SsIgnore : Attribute
{

}

[AttributeUsage(AttributeTargets.Property)]
public class SsItem : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SsCollection : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SsDictionary(Type keyType, Type valueType) : Attribute
{
    public string? Name { get; set; }

    public Type KeyType { get; } = keyType;

    public Type ValueType { get; } = valueType;
}