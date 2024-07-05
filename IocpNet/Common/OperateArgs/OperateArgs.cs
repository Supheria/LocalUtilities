using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

public abstract class OperateArgs(string timeStamp, string data) : ISsSerializable
{
    public string TimeStamp { get; protected set; } = timeStamp;

    public string Data { get; protected set; } = data;

    public abstract string LocalName { get; }

    public abstract void Serialize(SsSerializer serializer);

    public abstract void Deserialize(SsDeserializer deserializer);
}
