using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;

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
                if (item is not null)
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
    /// <param name="list"></param>
    public IList<TItem> Deserialize<TItem>(IList<TItem> list) where TItem : ISsSerializable, new()
    {
        list.Clear();
        if (Scope is null || !Scope.Property.TryGetValue(new TItem().LocalName, out var tokens) || tokens.Count is 0)
            return list;
        foreach (var token in tokens)
        {
            var deserializer = new SsDeserializer(new TItem());
            if (deserializer.Deserialize(token))
                list.Add((TItem)deserializer.Source);
        }
        return list;
    }

    /// <summary>
    /// read for collection to add all tokens of given type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="addToken"></param>
    public void Deserialize<TToken>(Action<TToken> addToken) where TToken : Token
    {
        if (Scope is null)
            return;
        foreach (var tokens in Scope.Property.Values)
        {
            foreach (var token in tokens)
            {
                if (token.GetType() == typeof(TToken))
                    addToken((TToken)token);
            }
        }
    }
}
