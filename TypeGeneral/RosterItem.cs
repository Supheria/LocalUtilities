using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public abstract class RosterItem<TSignature> : ISsSerializable where TSignature : notnull
{
    public abstract TSignature Signature { get; }

    public abstract string LocalName { get; }

    public abstract void Deserialize(SsDeserializer deserializer);

    public abstract void Serialize(SsSerializer serializer);
}
