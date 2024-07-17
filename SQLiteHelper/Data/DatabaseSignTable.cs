using LocalUtilities.SimpleScript.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SQLiteHelper.Data;

internal class DatabaseSignTable : SignTable
{
    public override char Empty { get; } = '\0';
    public override char Note { get; } = '@';
    public override char Quote { get; } = '#';
    public override char Escape { get; } = '\\';
    public override char Open { get; } = '{';
    public override char Close { get; } = '}';
    public override char Equal { get; } = '=';
    public override char Greater { get; } = '>';
    public override char Less { get; } = '<';
    public override char Tab { get; } = '\t';
    public override char Space { get; } = ' ';
    public override char Return { get; } = '\r';
    public override char NewLine { get; } = '\n';
}
