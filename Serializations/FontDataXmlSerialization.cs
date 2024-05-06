using LocalUtilities.SerializeUtilities;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public class FontDataXmlSerialization(string localName) : XmlSerialization<FontData>(new())
{
    public override string LocalName => localName;

    public FontDataXmlSerialization() : this(nameof(FontData))
    {

    }

    public override void ReadXml(XmlReader reader)
    {
        Source.FamilyName = reader.GetAttribute(nameof(Source.FamilyName)) ?? Source.FamilyName;
        Source.ScaleFactorToHeight = reader.GetAttribute(nameof(Source.ScaleFactorToHeight)).ToFloat() ?? Source.ScaleFactorToHeight;
        Source.Style = reader.GetAttribute(nameof(Source.Style)).ToEnum<FontStyle>();
        Source.Unit = reader.GetAttribute(nameof(Source.Unit)).ToEnum<GraphicsUnit>();
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString(nameof(Source.FamilyName), Source.FamilyName);
        writer.WriteAttributeString(nameof(Source.ScaleFactorToHeight), Source.ScaleFactorToHeight.ToString());
        writer.WriteAttributeString(nameof(Source.Style), Source.Style.ToString());
        writer.WriteAttributeString(nameof(Source.Unit), Source.Unit.ToString());
    }
}
