using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;

namespace LocalUtilities.Serializations;

internal class FontDataSerialization : SsSerialization<FontData>
{
    public override string LocalName { get; }

    public FontDataSerialization(string localName) : base(new())
    {
        LocalName += localName;
        OnSerialize += FontData_Serialize;
        OnDeserialize += FontData_Deserialize;
    }

    private void FontData_Serialize()
    {
        WriteTag(nameof(Source.FamilyName), Source.FamilyName);
        WriteTag(nameof(Source.ScaleFactorToHeight), Source.ScaleFactorToHeight.ToString());
        WriteTag(nameof(Source.Style), Source.Style.ToString());
        WriteTag(nameof(Source.Unit), Source.Unit.ToString());
    }

    private void FontData_Deserialize()
    {
        Source.FamilyName = ReadTag(nameof(Source.FamilyName), s => s ?? Source.FamilyName);
        Source.ScaleFactorToHeight = ReadTag(nameof(Source.ScaleFactorToHeight), s => s.ToFloat(Source.ScaleFactorToHeight));
        Source.Style = ReadTag(nameof(Source.Style), s => s.ToEnum(Source.Style));
        Source.Unit = ReadTag(nameof(Source.Unit), s => s.ToEnum(Source.Unit));
    }
}
