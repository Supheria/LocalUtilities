using LocalUtilities.TypeGeneral;

namespace LocalUtilities.SQLiteHelper.Data;

public class Condition : IRosterItem<string>
{
    public string FieldName { get; }

    public string PropertyName { get; }

    public object? Value { get; set; }

    public Keywords Operate { get; private set; }

    public string Signature => PropertyName;

    public Condition(string fieldName, string propertyName, object? value, Operators operate)
    {
        FieldName = fieldName;
        PropertyName = propertyName;
        Value = value;
        Operate = GetOperate(operate);
    }

    public Condition(string fieldName, object? value, Operators operate) : this(fieldName, fieldName, value, operate)
    {

    }

    public Condition(FieldName? field, object? value, Operators operate) : this(field?.Name ?? "", field?.PropertyName ?? "", value, operate)
    {

    }

    private Keywords GetOperate(Operators operate)
    {
        return operate switch
        {
            Operators.Equal => Keywords.Equal,
            Operators.LessThan => Keywords.Less,
            Operators.GreaterThan => Keywords.Greater,
            Operators.LessThanOrEqualTo => Keywords.LessOrEqual,
            Operators.GreaterThanOrEqualTo => Keywords.GreaterOrEqual,
            _ => Keywords.Blank
        };
    }

    public void SetOperate(Operators operate)
    {
        Operate = GetOperate(operate);
    }

    //public Condition(FieldValue? fieldValue, Operators operate)
    //{
    //    Name = fieldValue?.Name ?? "";
    //    Value = fieldValue?.Value;
    //    Operate = GetOperate(operate);
    //}
}