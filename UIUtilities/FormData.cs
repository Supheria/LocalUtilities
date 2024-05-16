using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeBundle;

namespace LocalUtilities.UIUtilities;

public abstract class FormData(string localName) : ISsSerializable
{
    public string LocalName { get; set; } = localName;

    public abstract Size MinimumSize { get; set; }

    public virtual Size Size { get; set; }

    public virtual Point Location { get; set; }

    public virtual FormWindowState WindowState { get; set; } = FormWindowState.Normal;

    public virtual int Padding { get; set; } = 12;

    public virtual FontData LabelFontData { get; set; } = new(nameof(LabelFontData));

    public virtual FontData ContentFontData { get; set; } = new(nameof(ContentFontData)) { ScaleFactorToHeight = 0.05f };

    protected abstract void SerializeFormData(SsSerializer serializer);

    protected abstract void DeserializeFormData(SsDeserializer deserializer);

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(MinimumSize), MinimumSize.ToArrayString());
        serializer.WriteTag(nameof(Size), Size.ToArrayString());
        serializer.WriteTag(nameof(Location), Location.ToArrayString());
        serializer.WriteTag(nameof(WindowState), WindowState.ToString());
        serializer.WriteTag(nameof(Padding), Padding.ToString());
        serializer.WriteObject(LabelFontData);
        serializer.WriteObject(ContentFontData);
        SerializeFormData(serializer);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        MinimumSize = deserializer.ReadTag(nameof(MinimumSize), s => s.ToSize(MinimumSize));
        Size = deserializer.ReadTag(nameof(Size), s => s.ToSize(Size));
        Location = deserializer.ReadTag(nameof(Location), s => s.ToPoint(Location));
        WindowState = deserializer.ReadTag(nameof(WindowState), s => s.ToEnum(WindowState));
        Padding = deserializer.ReadTag(nameof(Padding), s => s.ToInt(Padding));
        deserializer.ReadObject(LabelFontData);
        deserializer.ReadObject(ContentFontData);
        DeserializeFormData(deserializer);
    }
}