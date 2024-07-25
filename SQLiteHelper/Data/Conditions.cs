using LocalUtilities.TypeGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

/// <summary>
/// <para>roster of <see cref="Condition"/> using key of property name</para>
/// <para>use key index to change certain <see cref="Condition.Operate"/> value</para>
/// </summary>
public class Conditions : Roster<string, Condition>
{
}
