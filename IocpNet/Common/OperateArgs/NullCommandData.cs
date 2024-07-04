using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.IocpNet.Common.OperateArgs;

internal class NullCommandData : ICommandData
{
    public string ShortLog => StringTable.Null;

    public string LocalName => nameof(NullCommandData);

    public void Deserialize(SsDeserializer deserializer)
    {

    }

    public void Serialize(SsSerializer serializer)
    {

    }
}
