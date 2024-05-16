using LocalUtilities.FileUtilities;

namespace LocalUtilities.SimpleScript.Serialization;

public interface ISsSerializable
{
    public string LocalName { get; set; }

    public void Serialize(SsSerializer serializer);

    public void Deserialize(SsDeserializer deserializer);
}
