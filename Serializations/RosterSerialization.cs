using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.Serializations;

public abstract class RosterSerialization<TRoster, TSignature, TItem> : SsSerialization<TRoster>
    where TRoster : Roster<TSignature, TItem>, new() where TItem : RosterItem<TSignature>, new() where TSignature : notnull
{
    SsSerialization<TItem> ItemSerialization { get; }

    protected new SerializationOnRunning? OnSerialize { get; set; } = null;

    protected new SerializationOnRunning? OnDeserialize { get; set; } = null;

    public RosterSerialization(TRoster source, SsSerialization<TItem> itemXmlSerialzition)
    {
        ItemSerialization = itemXmlSerialzition;
        base.OnSerialize += Serialize;
        base.OnDeserialize += Deserialize;
    }

    private void Serialize()
    {
        OnSerialize?.Invoke();
        Serialize(Source.RosterList, ItemSerialization);
    }

    private void Deserialize()
    {
        OnDeserialize?.Invoke();
        Deserialize(ItemSerialization.LocalName, token =>
        {
            if(ItemSerialization.Deserialize(token))
                Source.Add(ItemSerialization.Source);
        });
    }
}
