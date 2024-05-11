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

    private void FontData_Serialize(SsSerializer serializer)
    {
        serializer.AppendTag(nameof(Source.FamilyName), Source.FamilyName);
        serializer.AppendTag(nameof(Source.ScaleFactorToHeight), Source.ScaleFactorToHeight.ToString());
        serializer.AppendTag(nameof(Source.Style), Source.Style.ToString());
        serializer.AppendTag(nameof(Source.Unit), Source.Unit.ToString());
    }

    private void FontData_Deserialize(Token token)
    {
        if (token is TagValues tagValues)
        {
            if (token.Name is nameof(Source.FamilyName))
                Source.FamilyName = tagValues.Tag;
            else if (token.Name is nameof(Source.ScaleFactorToHeight))
                Source.ScaleFactorToHeight = tagValues.Tag.ToFloat(Source.ScaleFactorToHeight);
            else if (token.Name is nameof(Source.Style))
                Source.Style = tagValues.Tag.ToEnum<FontStyle>();
            else if (token.Name is nameof(Source.Unit))
                Source.Unit = tagValues.Tag.ToEnum<GraphicsUnit>();
        }
    }
}
