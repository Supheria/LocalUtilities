using LocalUtilities.Interface;
using System.Xml;

namespace LocalUtilities.SerializeUtilities;

public class RosterXmlSerialization<TRoster, TItem> : XmlSerialization<TRoster> where TRoster : Roster<TItem> where TItem : IRosterItem
{
    XmlSerialization<TItem> _itemXmlSerialization;

    public RosterXmlSerialization(string localRootName, XmlSerialization<TItem> itemXmlSerialzition, TRoster source)
        : base(localRootName)
    {
        Source = source;
        _itemXmlSerialization = itemXmlSerialzition;
    }

    public override void ReadXml(XmlReader reader)
    {
        if (Source is null)
            return;
        var rosterList = new List<TItem>();
        rosterList.ReadXmlCollection(reader, LocalRootName, _itemXmlSerialization);
        Source.SetRoster(rosterList.ToArray());
    }

    public override void WriteXml(XmlWriter writer)
    {
        if (Source is null)
            return;
        Source.RosterList.WriteXmlCollection(writer, _itemXmlSerialization);
    }
}
