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

    public void DoSerialize(SsSerializer serializer)
    {
        serializer.AppendNameStart(LocalName);
        OnSerialize?.Invoke(serializer);
        serializer.AppendNameEnd();
    }

    public void DoDeserialize(Token token)
    {
        if (token is not Scope scope)
            return;
        foreach (var property in scope.Property)
            OnDeserialize?.Invoke(property);
    }
}
