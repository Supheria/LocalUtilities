using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SsSerialization<T>
{
    public T Parse(string str, out string? message)
    {
        try
        {
            message = null;
            foreach (var token in new Tokenizer(str).Tokens)
            {
                if (Deserialize(token))
                    return Source;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return Source;
    }

    public T LoadFromFile(out string? message, string? inFilePath = null)
    {
        try
        {
            var path = inFilePath ?? this.GetInitializationFilePath();
            if (!File.Exists(path))
                throw new SsParseExceptions($"could not open file: {path}");
            message = null;
            byte[] buffer;
            using var file = File.OpenRead(path);
            if (file.ReadByte() == 0xEF && file.ReadByte() == 0xBB && file.ReadByte() == 0xBF)
            {
                buffer = new byte[file.Length - 3];
                _ = file.Read(buffer, 0, buffer.Length);
            }
            else
            {
                file.Seek(0, SeekOrigin.Begin);
                buffer = new byte[file.Length];
                _ = file.Read(buffer, 0, buffer.Length);
            }
            foreach (var token in new Tokenizer(buffer).Tokens)
            {
                if (Deserialize(token))
                    return Source;
            }
            throw new SsParseExceptions($"cannot find an entry of {LocalName}");
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return Source;
    }

    /// <summary>
    /// read begin of this
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool Deserialize(Token token)
    {
        Source = new();
        //Source.Comments.AddRange(token.Comments);
        if (token.Name.Text == LocalName && token is Scope scope)
        {
            Scope = scope;
            Deserialize();
            return true;
        }
        return false;
    }

    /// <summary>
    /// read begin of successor
    /// </summary>
    protected abstract void Deserialize();

    /// <summary>
    /// read for single assignmented name of a tag
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="name"></param>
    /// <param name="toProperty">need default value for type convert failure</param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    protected TProperty ReadTag<TProperty>(string name, Func<string?, TProperty> toProperty)
    {
        string? str = null;
        if (Scope is null || !Scope.Property.TryGetValue(name, out var tokens) || tokens.Count is 0)
            return toProperty(str);
        var count = 0;
        foreach (var token in tokens)
        {
            if (token is TagValues tagValues)
            {
                str = tagValues.Tag.Text;
                if (++count > 1)
                    throw new SsParseExceptions(SsParseExceptions.MultiAssignment(tagValues.Name));
            }
        }
        return toProperty(str);
    }

    /// <summary>
    /// read for property of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="default"></param>
    /// <param name="serialization"></param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    protected TProperty Deserialize<TProperty>(TProperty @default, SsSerialization<TProperty> serialization) where TProperty : new()
    {
        if (Scope is null || !Scope.Property.TryGetValue(serialization.LocalName, out var tokens) || tokens.Count is 0)
            return @default;
        var count = 0;
        foreach (var token in tokens)
        {
            if (token is Scope scope)
            {
                serialization.Deserialize(scope);
                if (++count > 1)
                    throw new SsParseExceptions(SsParseExceptions.MultiAssignment(scope.Name));
            }
        }
        var source = serialization.Source;
        serialization.Source = new();
        return source;
    }

    /// <summary>
    /// read for collection of given type to add all items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="itemSerialization"></param>
    /// <param name="collection"></param>
    protected void Deserialize<TItem>(SsSerialization<TItem> itemSerialization, ICollection<TItem> collection) where TItem : new()
    {
        if (Scope is null || !Scope.Property.TryGetValue(itemSerialization.LocalName, out var tokens) || tokens.Count is 0)
            return;
        foreach (var token in tokens)
        {
            if (itemSerialization.Deserialize(token))
                collection.Add(itemSerialization.Source);
        }
    }

    /// <summary>
    /// read for collection to add all tokens of given type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="addToken"></param>
    protected void Deserialize(Type type, Action<Token> addToken)
    {
        if (Scope is null)
            return;
        foreach (var tokens in Scope.Property.Values)
        {
            foreach (var token in tokens)
            {
                if (token.GetType() == type)
                    addToken(token);
            }
        }
    }
}
