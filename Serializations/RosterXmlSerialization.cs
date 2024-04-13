using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class RosterXmlSerialization<TRoster, TItem> : XmlSerialization<TRoster>
    where TRoster : Roster<TItem> where TItem : IRosterItem
{
    XmlSerialization<TItem> ItemXmlSerialization { get; }

    public RosterXmlSerialization(TRoster source, XmlSerialization<TItem> itemXmlSerialzition)
    {
        Source = source;
        ItemXmlSerialization = itemXmlSerialzition;
    }

    public override void ReadXml(XmlReader reader)
    {
        if (Source is null)
            return;
        var rosterList = new List<TItem>();
        rosterList.ReadXmlCollection(reader, LocalName, ItemXmlSerialization);
        Source.SetRoster(rosterList.ToArray());
    }

    public override void WriteXml(XmlWriter writer)
    {
        if (Source is null)
            return;
        Source.RosterList.WriteXmlCollection(writer, ItemXmlSerialization);
    }
}
