using LocalUtilities.Interface;
using LocalUtilities.SimpleScript.Data;

namespace LocalUtilities.SimpleScript.Serialization;

public abstract partial class SsSerialization<T> : IInitializeable where T : new()
{
    public T Source { get; set; } = new();

    public abstract string LocalName { get; }

    public string IniFileName => LocalName;

    public string IniFileExtension { get; } = "ss";

    SsSerializer? Serializer { get; set; }

    Scope? Scope { get; set; } = null;
}
