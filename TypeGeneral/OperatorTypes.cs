namespace LocalUtilities.TypeGeneral;

public enum OperatorTypes
{
    Equal,
    LessThan,
    GreaterThan,
    LessThanOrEqualTo,
    GreaterThanOrEqualTo,
}

public static class OperateTool
{
    public static char ToChar(this OperatorTypes type)
    {
        return type switch
        {
            OperatorTypes.Equal => '=',
            OperatorTypes.LessThan => '<',
            OperatorTypes.GreaterThan => '>',
            _ => '\0'
        };
    }
}