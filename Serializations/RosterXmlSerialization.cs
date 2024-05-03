using LocalUtilities.DelegateUtilities;
using LocalUtilities.Interface;
using LocalUtilities.SerializeUtilities;
using System.Xml;

namespace LocalUtilities.Serializations;

public abstract class RosterXmlSerialization<TRoster, TSignature, TItem> : XmlSerialization<TRoster>
    where TRoster : Roster<TSignature, TItem> where TItem : RosterItem<TSignature> where TSignature : notnull
{
    protected event XmlReaderDelegate? OnRead;

    protected event XmlWriterDelegate? OnWrite;

    protected abstract string RosterName { get; }

    XmlSerialization<TItem> ItemXmlSerialization { get; }

    public RosterXmlSerialization(TRoster source, XmlSerialization<TItem> itemXmlSerialzition) : base(source)
    {
        ItemXmlSerialization = itemXmlSerialzition;
    }

    public override void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.Name == LocalName && reader.NodeType is XmlNodeType.EndElement)
                break;
            if (reader.NodeType is not XmlNodeType.Element)
                continue;
            if (reader.Name == RosterName)
                Source.RosterList.ReadXmlCollection(reader, ItemXmlSerialization, RosterName);
            OnRead?.Invoke(reader);
        }
    }

    public override void WriteXml(XmlWriter writer)
    {
        Source.RosterList.WriteXmlCollection(writer, ItemXmlSerialization, RosterName);
        OnWrite?.Invoke(writer);
    }
}
