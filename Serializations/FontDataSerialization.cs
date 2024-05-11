using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;

namespace LocalUtilities.Serializations;

internal class FontDataSerialization : SsSerialization<FontData>
{
    public override string LocalName { get; }

    public FontDataSerialization(string localName)
    {
        LocalName += localName;
        OnSerialize += Serialize;
        OnDeserialize += Deserialize;
    }

    private void Serialize()
    {
        WriteTag(nameof(Source.FamilyName), Source.FamilyName);
        WriteTag(nameof(Source.ScaleFactorToHeight), Source.ScaleFactorToHeight.ToString());
        WriteTag(nameof(Source.Style), Source.Style.ToString());
        WriteTag(nameof(Source.Unit), Source.Unit.ToString());
    }

    private void Deserialize()
    {
        Source.FamilyName = ReadTag(nameof(Source.FamilyName), s => s ?? Source.FamilyName);
        Source.ScaleFactorToHeight = ReadTag(nameof(Source.ScaleFactorToHeight), s => s.ToFloat(Source.ScaleFactorToHeight));
        Source.Style = ReadTag(nameof(Source.Style), s => s.ToEnum(Source.Style));
        Source.Unit = ReadTag(nameof(Source.Unit), s => s.ToEnum(Source.Unit));
    }
}
