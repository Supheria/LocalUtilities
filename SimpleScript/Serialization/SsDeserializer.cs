using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using LocalUtilities.TypeBundle;
using System.Collections;
using System.Collections.Generic;

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
            if (token is TagValue tagValues)
            {
                str = tagValues.Tag.Text;
                if (++count > 1)
                    throw SsParseExceptions.MultiAssignment(tagValues.Name);
            }
        }
        return toProperty(str);
    }

    public IList<T> ReadTags<T>(string name, IList<T> list, Func<string, T?> toProperty)
    {
        list.Clear();
        return GeneralReadCollection(name, list, tagValue =>
        {
            var tag = toProperty(tagValue.Tag.Text);
            if (tag is not null)
                list.Add(tag);
        });
    }

    public IList<KeyValuePair<TTag, List<TValue>>> ReadTagValues<TTag, TValue>(string name, Func<string, TTag> toTag, Func<string, TValue> toValue)
    {
        var list = new List<KeyValuePair<TTag, List<TValue>>>();
        return GeneralReadCollection(name, list, tagValue =>
        {
            var tag = toTag(tagValue.Tag.Text);
            var value = tagValue.Value.Select(x => toValue(x)).ToList();
            list.Add(new(tag, value));
        });
    }

    public IList<TItem> ReadValues<TItem>(string name, IList<TItem> items, Func<List<string>, TItem?> toItem)
    {
        items.Clear();
        return GeneralReadCollection(name, items, tagValue =>
        {
            var item = toItem(tagValue.Value);
            if (item is not null)
                items.Add(item);
        });
    }

    private T GeneralReadCollection<T>(string name, T collection, Action<TagValue> add)
    {
        if (Scope is null || !Scope.Property.TryGetValue(name, out var tokens) || tokens.Count is 0)
            return collection;
        foreach (var token in tokens)
        {
            if (token is TagValue tagValue)
                add(tagValue);
        }
        return collection;
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
}
