namespace LocalUtilities.SimpleScript.Serialization;

public interface ISsSerializable
{
    public string LocalName { get; }

    public void Serialize(SsSerializer serializer);

    public void Deserialize(SsDeserializer deserializer);
}
