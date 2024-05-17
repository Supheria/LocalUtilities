using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeGeneral;

public abstract class FormData() : ISsSerializable
{
    public abstract string LocalName { get; set; }

    public abstract Size MinimumSize { get; set; }

    public virtual Size Size { get; set; }

    public virtual Point Location { get; set; }

    public virtual FormWindowState WindowState { get; set; } = FormWindowState.Normal;

    protected abstract void SerializeFormData(SsSerializer serializer);

    protected abstract void DeserializeFormData(SsDeserializer deserializer);

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(MinimumSize), MinimumSize.ToArrayString());
        serializer.WriteTag(nameof(Size), Size.ToArrayString());
        serializer.WriteTag(nameof(Location), Location.ToArrayString());
        serializer.WriteTag(nameof(WindowState), WindowState.ToString());
        SerializeFormData(serializer);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        MinimumSize = deserializer.ReadTag(nameof(MinimumSize), s => s.ToSize());
        Size = deserializer.ReadTag(nameof(Size), s => s.ToSize());
        Location = deserializer.ReadTag(nameof(Location), s => s.ToPoint());
        WindowState = deserializer.ReadTag(nameof(WindowState), s => s.ToEnum<FormWindowState>());
        DeserializeFormData(deserializer);
    }
}
