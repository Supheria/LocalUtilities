using LocalUtilities.TypeGeneral;

namespace LocalUtilities.SQLiteHelper.Data;

/// <summary>
/// <para>roster of <see cref="Condition"/> using key of property name</para>
/// <para>use key index to change certain <see cref="Condition.Operate"/> value</para>
/// </summary>
public class ConditionRoster : Roster<string, Condition>
{
}
