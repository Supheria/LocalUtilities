namespace LocalUtilities.SQLiteHelper.Data;

public class Condition(string key, object value, Condition.Operates operate)
{
    public enum Operates
    {
        Equal,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual,
    }

    public enum Combo
    {
        Default,
        Or,
        And,
    }

    public string Key { get; } = key;

    public object Value { get; } = value;

    public Keywords Operate { get; } = operate switch
    {
        Operates.Equal => Keywords.Equal,
        Operates.Less => Keywords.Less,
        Operates.Greater => Keywords.Greater,
        Operates.LessOrEqual => Keywords.LessOrEqual,
        Operates.GreaterOrEqual => Keywords.GreaterOrEqual,
        _ => Keywords.Blank
    };
}