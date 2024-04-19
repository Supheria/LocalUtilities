using LocalUtilities.Interface;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public abstract class XmlSerialization<T>(T source) : IXmlSerializable, IInitializeable
{
    public T Source = source;

    public abstract string LocalName { get; }

    public virtual string? RootElementNamespace { get; } = null;

    public string? IniFileName { get; set; }

    public XmlSchema? GetSchema() => null;

    public abstract void ReadXml(XmlReader reader);

    public abstract void WriteXml(XmlWriter writer);
}