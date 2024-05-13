using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsDeserializer(object obj) : SsSerializeBase(obj)
{
    Scope? Scope { get; set; } = null;

    /// <summary>
    /// read begin of this
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool Deserialize(Token token)
    {
        //Source = new();
        //Source.Comments.AddRange(token.Comments);
        if (token.Name.Text == Source.LocalName && token is Scope scope)
        {
            Scope = scope;
            Source.Deserialize(this);
            return true;
        }
        return false;
    }

    /// <summary>
    /// read for single assignmented name of a tag
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="toProperty">need default value for type convert failure</param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    public T ReadTag<T>(string name, Func<string?, T> toProperty)
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
                    throw SsParseExceptions.MultiAssignment(tagValues.Name);
            }
        }
        return toProperty(str);
    }

    public void ReadTags<T>(string name, ICollection<T> collection, Func<string?, T?> toProperty)
    {
        if (Scope is null || !Scope.Property.TryGetValue(name, out var tokens) || tokens.Count is 0)
            return;
        foreach (var token in tokens)
        {
            if (token is TagValues tagValues)
            {
                var item = toProperty(tagValues.Tag.Text);
                if(item is not null)
                    collection.Add(item);
            }
        }
    }

    /// <summary>
    /// read for property of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="default"></param>
    /// <param name="serialization"></param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    public TProperty Deserialize<TProperty>(TProperty @default) where TProperty : ISsSerializable
    {
        if (Scope is null || !Scope.Property.TryGetValue(@default.LocalName, out var tokens) || tokens.Count is 0)
            return @default;
        var count = 0;
        foreach (var token in tokens)
        {
            if (token is Scope scope)
            {
                var deserializer = new SsDeserializer(@default);
                deserializer.Deserialize(scope);
                if (++count > 1)
                    throw SsParseExceptions.MultiAssignment(scope.Name);
            }
        }
        return @default;
    }

    /// <summary>
    /// read for collection of given type to add all items
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="Deserializer"></param>
    /// <param name="collection"></param>
    public void Deserialize<TItem>(TItem sample, ICollection<TItem> collection) where TItem : ISsSerializable, new()
    {
        collection.Clear();
        if (Scope is null || !Scope.Property.TryGetValue(sample.LocalName, out var tokens) || tokens.Count is 0)
            return;
        foreach (var token in tokens)
        {
            var deserializer = new SsDeserializer(new TItem { LocalName = sample.LocalName });
            if (deserializer.Deserialize(token))
                collection.Add((TItem)deserializer.Source);
        }
    }

    public void Deserialize<TItem>(TItem sample, Action<TItem> addItem) where TItem : ISsSerializable, new()
    {
        if (Scope is null || !Scope.Property.TryGetValue(sample.LocalName, out var tokens) || tokens.Count is 0)
            return;
        foreach (var token in tokens)
        {
            var deserializer = new SsDeserializer(new TItem { LocalName = sample.LocalName });
            if (deserializer.Deserialize(token))
                addItem((TItem)deserializer.Source);
        }
    }

    /// <summary>
    /// read for collection to add all tokens of given type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="addToken"></param>
    public void Deserialize(Type type, Action<Token> addToken)
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
