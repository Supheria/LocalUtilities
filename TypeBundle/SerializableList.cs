using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeBundle;

public class SerializableList<TItem>(string localName, List<TItem> collection) : ISsSerializable where TItem : ISsSerializable, new()
{
    public List<TItem> List { get; private set; } = collection;

    public string LocalName { get; set; } = localName;

    public SerializableList(string localName) : this(localName, [])
    {

    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteSerializableItems(List);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        List = deserializer.ReadSerializableItems<TItem>();
    }
}
