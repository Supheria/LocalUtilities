using LocalUtilities.Interface;
using LocalUtilities.SimpleScript.Data;

namespace LocalUtilities.SimpleScript.Serialization;

public delegate void SsSerializeDelegate(SsSerializer serializer);

public delegate void SsDeserializeDelegate(Token token);

public abstract class SsSerialization<T>(T source) : IInitializeable
{
    public T Source = source;

    public abstract string LocalName { get; }

    public string? IniFileName { get; set; } = null;

    public string IniFileExtension { get; } = "ss";

    protected SsSerializeDelegate? OnSerialize { get; set; } = null;

    protected SsDeserializeDelegate? OnDeserialize { get; set; } = null;

    internal void BeginSerialize(SsSerializer serializer)
    {
        serializer.AppendNameStart(LocalName);
        OnSerialize?.Invoke(serializer);
        serializer.AppendNameEnd();
    }

    internal void BeginDeserialize(Token token)
    {
        if (token is Scope scope)
            scope.Property.ForEach(p => OnDeserialize?.Invoke(p));
    }
}
