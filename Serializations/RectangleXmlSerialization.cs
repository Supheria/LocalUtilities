using LocalUtilities.SerializeUtilities;
using LocalUtilities.StringUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public class RectangleXmlSerialization(string localName) : XmlSerialization<Rectangle>()
{
    public override string LocalName => localName;

    public RectangleXmlSerialization() : this(nameof(Rectangle))
    {

    }

    public override void ReadXml(XmlReader reader)
    {
        Source.X = reader.GetAttribute(nameof(Source.Left)).ToInt() ?? Source.Left;
        Source.Y = reader.GetAttribute(nameof(Source.Top)).ToInt() ?? Source.Top;
        Source.Width = reader.GetAttribute(nameof(Source.Width)).ToInt() ?? Source.Width;
        Source.Height = reader.GetAttribute(nameof(Source.Height)).ToInt() ?? Source.Height;
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString(nameof(Source.Left), Source.Left.ToString());
        writer.WriteAttributeString(nameof(Source.Top), Source.Top.ToString());
        writer.WriteAttributeString(nameof(Source.Width), Source.Width.ToString());
        writer.WriteAttributeString(nameof(Source.Height), Source.Height.ToString());
    }
}
