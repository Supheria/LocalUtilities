using System.Text;

namespace LocalUtilities.SimpleScript;

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
