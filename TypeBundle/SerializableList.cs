using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeBundle;

public class SerializableList<TItem>(string localName, IList<TItem> collection) : ISsSerializable where TItem : ISsSerializable, new()
{
    public IList<TItem> List { get; } = collection;

    public string LocalName { get; set; } = localName;

    public SerializableList(string localName) : this(localName, [])
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.Serialize(List);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        deserializer.Deserialize(List);
    }
}
