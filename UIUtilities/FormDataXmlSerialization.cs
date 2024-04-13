using LocalUtilities.SerializeUtilities;
using LocalUtilities.StringUtilities;
using System.Xml;

namespace LocalUtilities.UIUtilities;

public class FormDataXmlSerialization(string localName) : XmlSerialization<FormData>()
{
    protected XmlReaderDelegate? OnRead { get; }

    protected XmlWriterDelegate? OnWrite { get; }

    public override string LocalName => localName;

    public FormDataXmlSerialization() : this(nameof(FormData))
    {

    }

    public override void ReadXml(XmlReader reader)
    {
        OnRead?.Invoke(reader);
        Source ??= new();
        var size = reader.GetAttribute(nameof(Source.Size)).ToArray();
        var location = reader.GetAttribute(nameof(Source.Location)).ToArray();
        Source.Size = size.Length > 1
            ? new(size[0].ToInt() ?? 0, size[1].ToInt() ?? 0)
            : new();
        Source.Location = location.Length > 1
            ? new(location[0].ToInt() ?? 0, location[1].ToInt() ?? 0)
            : new();
        Source.WindowState = reader.GetAttribute(nameof(Source.WindowState)).ToEnum<FormWindowState>();
    }

    public override void WriteXml(XmlWriter writer)
    {
        if (Source is null)
            return;
        OnWrite?.Invoke(writer);
        writer.WriteAttributeString(nameof(Source.Size),
            StringSimpleTypeConverter.ToArrayString(Source.Size.Width, Source.Size.Height));
        writer.WriteAttributeString(nameof(Source.Location),
            StringSimpleTypeConverter.ToArrayString(Source.Location.X, Source.Location.Y));
        writer.WriteAttributeString(nameof(Source.WindowState), Source.WindowState.ToString());
    }

}
