using LocalUtilities.DelegateUtilities;
using LocalUtilities.Serializations;
using LocalUtilities.SerializeUtilities;
using LocalUtilities.StringUtilities;
using System.Xml;

namespace LocalUtilities.UIUtilities;

public abstract class FormDataXmlSerialization<T>(string localName, T formData) : XmlSerialization<T>(formData) where T : FormData
{
    protected event XmlReaderDelegate? OnRead;

    protected event XmlWriterDelegate? OnWrite;
    public override string LocalName => localName;

    public override void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.Name == LocalName && reader.NodeType is XmlNodeType.EndElement)
                break;
            if (reader.NodeType is not XmlNodeType.Element)
                continue;
            if (reader.Name is nameof(Source.Size))
            {
                var size = reader.ReadElementContentAsString().ToArray();
                Source.Size = size.Length > 1
                    ? new(size[0].ToInt() ?? 0, size[1].ToInt() ?? 0)
                    : Source.Size;
            }
            if (reader.Name is nameof(Source.Location))
            {
                var location = reader.ReadElementContentAsString().ToArray();
                Source.Location = location.Length > 1
                    ? new(location[0].ToInt() ?? 0, location[1].ToInt() ?? 0)
                    : Source.Location;
            }
            if (reader.Name is nameof(Source.WindowState))
                Source.WindowState = reader.ReadElementContentAsString().ToEnum<FormWindowState>();
            if (reader.Name is nameof(Source.Padding))
                Source.Padding = reader.ReadElementContentAsString().ToInt() ?? Source.Padding;
            if (reader.Name is nameof(Source.LabelFontData))
                Source.LabelFontData = new FontDataXmlSerialization(nameof(Source.LabelFontData)).Deserialize(reader);
            if (reader.Name is nameof(Source.ContentFontData))
                Source.LabelFontData = new FontDataXmlSerialization(nameof(Source.ContentFontData)).Deserialize(reader);
            OnRead?.Invoke(reader);
        }
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteElementString(nameof(Source.Size), StringSimpleTypeConverter.ToArrayString(Source.Size.Width, Source.Size.Height));
        writer.WriteElementString(nameof(Source.Location), StringSimpleTypeConverter.ToArrayString(Source.Location.X, Source.Location.Y));
        writer.WriteElementString(nameof(Source.WindowState), Source.WindowState.ToString());
        writer.WriteElementString(nameof(Source.Padding), Source.Padding.ToString());
        new FontDataXmlSerialization(nameof(Source.LabelFontData)) { Source = Source.LabelFontData }.Serialize(writer);
        new FontDataXmlSerialization(nameof(Source.ContentFontData)) { Source = Source.ContentFontData }.Serialize(writer);
        OnWrite?.Invoke(writer);
    }

}
