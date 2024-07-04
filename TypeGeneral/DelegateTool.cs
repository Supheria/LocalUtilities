using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeGeneral;

public delegate void SerializeHandler(SsSerializer serializer);

public delegate void DeserializeHandler(SsDeserializer deserializer);
