using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalUtilities.SQLiteHelper.Data;

namespace LocalUtilities.SQLiteHelper;

internal static class SQLiteExceptionEx
{
    public static void ThrowIfFieldCountNotMatch(this SQLiteDataReader reader, Volume[] values)
    {
        if (reader.FieldCount != values.Length)
            throw new SQLiteException("insert value number is not match to column count");
    }
}
