using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public abstract class RosterItem<TSignature>(TSignature signature) : ISsSerializable where TSignature : notnull
{
    public TSignature Signature { get; protected set; } = signature;

    public TSignature SetSignature { set => Signature = value; }

    public abstract string LocalName { get; set; }

    public abstract void Deserialize(SsDeserializer deserializer);

    public abstract void Serialize(SsSerializer serializer);
}
