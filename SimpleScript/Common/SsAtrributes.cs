using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Common;

//[AttributeUsage(AttributeTargets.Property)]
public class SsItem : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class SsIgnore : Attribute
{

}