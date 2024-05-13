using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeBundle;

namespace LocalUtilities.UIUtilities;

public class FontData(string localName) : ISsSerializable
{
    public string LocalName { get; set; } = localName;

    public string FamilyName { get; set; } = "黑体";

    public float ScaleFactorToHeight { get; set; } = 0.03f;

    public FontStyle Style { get; set; } = FontStyle.Regular;

    public GraphicsUnit Unit { get; set; } = GraphicsUnit.Pixel;

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(FamilyName), FamilyName);
        serializer.WriteTag(nameof(ScaleFactorToHeight), ScaleFactorToHeight.ToString());
        serializer.WriteTag(nameof(Style), Style.ToString());
        serializer.WriteTag(nameof(Unit), Unit.ToString());
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        FamilyName = deserializer.ReadTag(nameof(FamilyName), s => s ?? FamilyName);
        ScaleFactorToHeight = deserializer.ReadTag(nameof(ScaleFactorToHeight), s => s.ToFloat(ScaleFactorToHeight));
        Style = deserializer.ReadTag(nameof(Style), s => s.ToEnum(Style));
        Unit = deserializer.ReadTag(nameof(Unit), s => s.ToEnum(Unit));
    }
}
