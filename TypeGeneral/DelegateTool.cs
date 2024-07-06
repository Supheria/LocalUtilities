using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public delegate void SerializeHandler(SsSerializer serializer);

public delegate void DeserializeHandler(SsDeserializer deserializer);
