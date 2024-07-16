namespace LocalUtilities.SimpleScript;

[AttributeUsage(AttributeTargets.Property)]
public class SsItem : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public class SsIgnore : Attribute
{

}
