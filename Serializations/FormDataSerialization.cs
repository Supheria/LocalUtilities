using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;
using static System.Formats.Asn1.AsnWriter;

namespace LocalUtilities.Serializations;

public class FormDataSerialization<T> : SsSerialization<T> where T : FormData
{
    public override string LocalName { get; }

    FontDataSerialization LabelFontDataSerialization { get; } = new(nameof(Source.LabelFontData));

    FontDataSerialization ContentFontDataSerialization { get; } = new(nameof(Source.ContentFontData));


    public FormDataSerialization(string localName, T formData) : base(formData)
    {
        LocalName = localName;
        OnSerialize += FormData_Serialize;
        OnDeserialize += FormData_Deserialize;
    }

    private void FormData_Serialize()
    {
        WriteTag(nameof(Source.MinimumSize), Source.MinimumSize.ToArrayString());
        WriteTag(nameof(Source.Size), Source.Size.ToArrayString());
        WriteTag(nameof(Source.Location), Source.Location.ToArrayString());
        WriteTag(nameof(Source.WindowState), Source.WindowState.ToString());
        WriteTag(nameof(Source.Padding), Source.Padding.ToString());
        Serialize(Source.LabelFontData, LabelFontDataSerialization);
        Serialize(Source.ContentFontData, ContentFontDataSerialization);
    }

    private void FormData_Deserialize()
    {
        Source.MinimumSize = ReadTag(nameof(Source.MinimumSize), s => s.ToSize(Source.MinimumSize));
        Source.Size = ReadTag(nameof(Source.Size), s => s.ToSize(Source.Size));
        Source.Location = ReadTag(nameof(Source.Location), s => s.ToPoint(Source.Location));
        Source.WindowState = ReadTag(nameof(Source.WindowState), s=>s.ToEnum(Source.WindowState));
        Source.Padding = ReadTag(nameof(Source.Padding), s => s.ToInt(Source.Padding));
        Source.LabelFontData = Deserialize(Source.LabelFontData, LabelFontDataSerialization);
        Source.ContentFontData = Deserialize(Source.ContentFontData, ContentFontDataSerialization);
    }
}
