using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class RosterXmlSerialization<TRoster, TItem>(XmlSerialization<TItem> itemXmlSerialzition, TRoster source)
    : XmlSerialization<TRoster>(source) where TRoster : Roster<TItem> where TItem : IRosterItem
{
    XmlSerialization<TItem> ItemXmlSerialization => itemXmlSerialzition;

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
