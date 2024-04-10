using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public abstract class XmlSerialization<T>(string localRootName) : IXmlSerializable
{
    public string LocalRootName { get; } = localRootName;

    public T? Source { get; set; } = default;

    public virtual XmlSchema? GetSchema() => null;

    public abstract void ReadXml(XmlReader reader);

    public abstract void WriteXml(XmlWriter writer);
}