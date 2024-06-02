using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeGeneral;

public class FontData(string localName) : ISsSerializable
{
    public string LocalName => localName;

    public string FamilyName { get; set; } = "黑体";

    public float Size { get; set; } = 15f;

    public FontStyle Style { get; set; } = FontStyle.Regular;

    public GraphicsUnit Unit { get; set; } = GraphicsUnit.Pixel;

    public FontData() : this(nameof(FontData))
    {

    }

    public static implicit operator Font(FontData data)
    {
        return new(data.FamilyName, data.Size, data.Style, data.Unit);
    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(FamilyName), FamilyName);
        serializer.WriteTag(nameof(Size), Size.ToString());
        serializer.WriteTag(nameof(Style), Style.ToString());
        serializer.WriteTag(nameof(Unit), Unit.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        FamilyName = deserializer.ReadTag(nameof(FamilyName), s => s);
        Size = deserializer.ReadTag(nameof(Size), float.Parse);
        Style = deserializer.ReadTag(nameof(Style), s => s.ToEnum<FontStyle>());
        Unit = deserializer.ReadTag(nameof(Unit), s => s.ToEnum<GraphicsUnit>());
    }
}
