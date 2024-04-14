using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class RosterXmlSerialization<TRoster, TItem>(TRoster source, XmlSerialization<TItem> itemXmlSerialzition)
    : XmlSerialization<TRoster>(source) where TRoster : Roster<TItem> where TItem : RosterItem
{
    XmlSerialization<TItem> ItemXmlSerialization { get; } = itemXmlSerialzition;

    public override void ReadXml(XmlReader reader)
    {
        var rosterList = new List<TItem>();
        rosterList.ReadXmlCollection(reader, LocalName, ItemXmlSerialization);
        Source.SetRoster(rosterList.ToArray());
    }

    public override void WriteXml(XmlWriter writer)
    {
        Source.RosterList.WriteXmlCollection(writer, ItemXmlSerialization);
    }
}
