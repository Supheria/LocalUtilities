using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;

namespace LocalUtilities.Serializations;

public abstract class FormDataSerialization<T>(string localName) : SsSerialization<T> where T : FormData, new()
{
    public override string LocalName { get; } = localName;

    FontDataSerialization LabelFontDataSerialization { get; } = new(nameof(Source.LabelFontData));

    FontDataSerialization ContentFontDataSerialization { get; } = new(nameof(Source.ContentFontData));

    protected sealed override void Serialize()
    {
        SerializeFormData();
        WriteTag(nameof(Source.MinimumSize), Source.MinimumSize.ToArrayString());
        WriteTag(nameof(Source.Size), Source.Size.ToArrayString());
        WriteTag(nameof(Source.Location), Source.Location.ToArrayString());
        WriteTag(nameof(Source.WindowState), Source.WindowState.ToString());
        WriteTag(nameof(Source.Padding), Source.Padding.ToString());
        Serialize(Source.LabelFontData, LabelFontDataSerialization);
        Serialize(Source.ContentFontData, ContentFontDataSerialization);
    }

    protected abstract void SerializeFormData();

    protected sealed override void Deserialize()
    {
        DeserializeFormData();
        Source.MinimumSize = ReadTag(nameof(Source.MinimumSize), s => s.ToSize(Source.MinimumSize));
        Source.Size = ReadTag(nameof(Source.Size), s => s.ToSize(Source.Size));
        Source.Location = ReadTag(nameof(Source.Location), s => s.ToPoint(Source.Location));
        Source.WindowState = ReadTag(nameof(Source.WindowState), s => s.ToEnum(Source.WindowState));
        Source.Padding = ReadTag(nameof(Source.Padding), s => s.ToInt(Source.Padding));
        Source.LabelFontData = Deserialize(Source.LabelFontData, LabelFontDataSerialization);
        Source.ContentFontData = Deserialize(Source.ContentFontData, ContentFontDataSerialization);
    }
    protected abstract void DeserializeFormData();
}
