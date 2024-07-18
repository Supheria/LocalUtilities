using LocalUtilities.SimpleScript.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.SimpleScript.Serializer;

internal class SsStringWriter(bool writeIntoMultiLines, SignTable signTable) : SsWriter(writeIntoMultiLines, signTable)
{
    StringBuilder Writer { get; } = new();

    protected override void WriteString(string str)
    {
        Writer.Append(str);
    }

    public override string ToString()
    {
        return Writer.ToString();
    }
}
