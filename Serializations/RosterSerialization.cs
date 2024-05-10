using LocalUtilities.DelegateUtilities;
using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class RosterSerialization<TRoster, TSignature, TItem> : SsSerialization<TRoster>
    where TRoster : Roster<TSignature, TItem> where TItem : RosterItem<TSignature> where TSignature : notnull
{
    SsSerialization<TItem> ItemXmlSerialization { get; }

    public RosterSerialization(TRoster source, SsSerialization<TItem> itemXmlSerialzition) : base(source)
    {
        ItemXmlSerialization = itemXmlSerialzition;
        OnSerialize += Roster_Serialize;
        OnDeserialize += Roster_Deserialize;
    }

    private void Roster_Serialize(SsSerializer serializer)
    {
        Source.RosterList.Serialize(serializer, ItemXmlSerialization);
    }

    private void Roster_Deserialize(Token token)
    {
        Source.Add(ItemXmlSerialization.Deserialize(token));
    }
}
