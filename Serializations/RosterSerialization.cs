using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.Serializations;

public abstract class RosterSerialization<TRoster, TSignature, TItem> : SsSerialization<TRoster>
    where TRoster : Roster<TSignature, TItem> where TItem : RosterItem<TSignature> where TSignature : notnull
{
    SsSerialization<TItem> ItemSerialization { get; }

    public RosterSerialization(TRoster source, SsSerialization<TItem> itemXmlSerialzition) : base(source)
    {
        ItemSerialization = itemXmlSerialzition;
        OnSerialize += Roster_Serialize;
        OnDeserialize += Roster_Deserialize;
    }

    private void Roster_Serialize()
    {
        Serialize(Source.RosterList, ItemSerialization);
    }

    private void Roster_Deserialize()
    {
        Deserialize(ItemSerialization.LocalName, token =>
        {
            if(ItemSerialization.Deserialize(token))
                Source.Add(ItemSerialization.Source);
        });
    }
}
