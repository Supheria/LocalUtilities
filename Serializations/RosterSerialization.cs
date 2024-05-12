using LocalUtilities.Interface;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.Serializations;

public abstract class RosterSerialization<TRoster, TSignature, TItem> : SsSerialization<TRoster>
    where TRoster : Roster<TSignature, TItem>, new() where TItem : RosterItem<TSignature>, new() where TSignature : notnull
{
    protected abstract SsSerialization<TItem> ItemSerialization { get; }

    protected sealed override void Serialize()
    {
        SerializeRoster();
        Serialize(Source, ItemSerialization);
    }

    protected abstract void SerializeRoster();

    protected sealed override void Deserialize()
    {
        DeserializeRoster();
        Deserialize(ItemSerialization, Source);
    }

    protected abstract void DeserializeRoster();
}
