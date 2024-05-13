using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.TypeBundle;

public class SerializableCollection<TItem>(string localName, ICollection<TItem> collection, TItem itemSample) : ISsSerializable where TItem : ISsSerializable, new()
{
    TItem ItemSample { get; } = itemSample;

    public ICollection<TItem> Collection { get; } = collection;

    public string LocalName { get; set; } = localName;

    public void Serialize(SsSerializer serializer)
    {
        serializer.Serialize(Collection);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        deserializer.Deserialize(ItemSample, Collection);
    }
}
