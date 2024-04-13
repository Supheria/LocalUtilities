using LocalUtilities.Interface;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LocalUtilities.SerializeUtilities;

public abstract class XmlSerialization<T>() : IXmlSerializable, IInitializeable
{
    public T? Source = default;

    public abstract string LocalName { get; }

    public virtual string? RootElementNamespace { get; } = null;

    public string IniFileName => LocalName;

    public XmlSchema? GetSchema() => null;

    public abstract void ReadXml(XmlReader reader);

    public abstract void WriteXml(XmlWriter writer);
}