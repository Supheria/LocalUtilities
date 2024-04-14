using LocalUtilities.SerializeUtilities;
using LocalUtilities.StringUtilities;
using System.Xml;

namespace LocalUtilities.UIUtilities;

public abstract class FormDataXmlSerialization<T>(string localName, T formData) : XmlSerialization<T>(formData) where T : FormData
{
    protected XmlReaderDelegate? OnRead { get; set; }

    protected XmlWriterDelegate? OnWrite { get; set; }

    public override string LocalName => localName;

    public override void ReadXml(XmlReader reader)
    {
        OnRead?.Invoke(reader);
        var size = reader.GetAttribute(nameof(Source.Size)).ToArray();
        var location = reader.GetAttribute(nameof(Source.Location)).ToArray();
        Source.Size = size.Length > 1
            ? new(size[0].ToInt() ?? 0, size[1].ToInt() ?? 0)
            : Source.Size;
        Source.Location = location.Length > 1
            ? new(location[0].ToInt() ?? 0, location[1].ToInt() ?? 0)
            : Source.Location;
        Source.WindowState = reader.GetAttribute(nameof(Source.WindowState)).ToEnum<FormWindowState>();
    }

    public override void WriteXml(XmlWriter writer)
    {
        OnWrite?.Invoke(writer);
        writer.WriteAttributeString(nameof(Source.Size),
            StringSimpleTypeConverter.ToArrayString(Source.Size.Width, Source.Size.Height));
        writer.WriteAttributeString(nameof(Source.Location),
            StringSimpleTypeConverter.ToArrayString(Source.Location.X, Source.Location.Y));
        writer.WriteAttributeString(nameof(Source.WindowState), Source.WindowState.ToString());
    }

}
