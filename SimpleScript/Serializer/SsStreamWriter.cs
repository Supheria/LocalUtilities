using LocalUtilities.SimpleScript.Common;
using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

internal class SsStreamWriter(Stream stream, Encoding encoding, bool writeIntoMultiLines, SignTable signTable) : SsWriter(writeIntoMultiLines, signTable), IDisposable
{
    Stream Stream { get; } = stream;

    Encoding Encoding { get; } = encoding;

    protected override void WriteString(string str)
    {
        var buffer = Encoding.GetBytes(str);
        Stream.Write(buffer);
    }

    public void Dispose()
    {
        Stream.Flush();
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
