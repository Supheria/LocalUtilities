﻿using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.SQLiteHelper.Data;

public class Volume
{
    public string Value { get; }

    public Volume(string value)
    {
        Value = new StringBuilder()
            .Append(SignCollection.Quote)
            .Append(value)
            .Append(SignCollection.Quote)
            .ToString();
    }

    public Volume(Keywords keywords)
    {
        Value = keywords.Value;
    }

    public override string ToString()
    {
        return Value;
    }
}