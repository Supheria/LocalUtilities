using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public abstract class XmlSerialization<T> : IXmlSerializable
{
    public T? Source { get; set; } = default;

    public string LocalRootName { get; }

    protected XmlSerialization(string localRootName) => LocalRootName = localRootName;

    public virtual XmlSchema? GetSchema() => null;

    public abstract void ReadXml(XmlReader reader);

    public abstract void WriteXml(XmlWriter writer);
}