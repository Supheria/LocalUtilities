using System.Text;

namespace LocalUtilities.SimpleScript.Serializer;

public class SsWriter(Stream stream, bool writeIntoMultiLines) : IDisposable
{
    Stream Stream { get; } = stream;

    int Level { get; set; } = 0;

    bool NameAppended { get; set; } = false;

    public bool WriteIntoMultiLines { get; } = writeIntoMultiLines;

    public void AppendName(string name)
    {
        Stream.WriteName(Level, name, WriteIntoMultiLines);
        NameAppended = true;
    }

    public void AppendStart()
    {
        Level++;
        if (NameAppended)
            Stream.WriteStart(0, WriteIntoMultiLines);
        else
            Stream.WriteStart(Level, WriteIntoMultiLines);
        NameAppended = false;
    }

    public void AppendEnd()
    {
        Stream.WriteEnd(--Level, WriteIntoMultiLines);
    }

    public void AppendValue(string value)
    {
        if (NameAppended)
            Stream.WriteValue(0, value, WriteIntoMultiLines);
        else
            Stream.WriteValue(Level, value, WriteIntoMultiLines);
        NameAppended = false;
    }

    public void Dispose()
    {
        Stream.Flush();
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
