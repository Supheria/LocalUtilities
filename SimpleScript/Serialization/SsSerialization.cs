using LocalUtilities.Interface;
using LocalUtilities.SimpleScript.Data;

namespace LocalUtilities.SimpleScript.Serialization;

public delegate void SsSerializeDelegate(SsSerializer writer);

public delegate void SsDeserializeDelegate(Token token);

public abstract class SsSerialization<T>(T source) : IInitializeable
{
    public T Source = source;

    public abstract string LocalName { get; }

    public string? IniFileName { get; set; } = null;

    public string IniFileExtension { get; } = "ss";

    protected SsSerializeDelegate? OnSerialize { get; set; } = null;

    protected SsDeserializeDelegate? OnDeserialize { get; set; } = null;

    public void Serializ(SsSerializer serializer)
    {
        serializer.WriteNameStart(LocalName);
        OnSerialize?.Invoke(serializer);
        serializer.WriteNameEnd();
    }

    public void Deserialize(Token token)
    {
        if (token is not Scope scope)
            return;
        foreach (var property in scope.Property)
            OnDeserialize?.Invoke(property);
    }
}
