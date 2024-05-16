using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Serialization;

public class SsDeserializer(object obj) : SsSerializeBase(obj)
{
    Dictionary<string, List<Element>> Elements { get; set; } = [];

    /// <summary>
    /// read begin of this
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public void Deserialize(Dictionary<string, List<Element>> elements)
    {
        Elements = elements;
        Source.Deserialize(this);
    }

    private List<TItem> GeneralReadList<TElement, TItem>(string name, Action<TElement, List<TItem>> addItem)
    {
        var list = new List<TItem>();
        if (!Elements.TryGetValue(name, out var elements) || elements.Count is 0)
            return list;
        foreach (var element in elements)
        {
            if (element is TElement e)
                addItem(e, list);
        }
        return list;
    }

    /// <summary>
    /// read for element like: xyz = str
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="toProperty">need default value for type convert failure</param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    public T ReadTag<T>(string name, Func<string?, T> toProperty)
    {
        string? str = null;
        if (!Elements.TryGetValue(name, out var elements) || elements.Count is 0)
            return toProperty(str);
        if (elements.Count > 1)
            throw SsParseExceptions.MultiAssignment(name);
        return toProperty(elements.First().Tag.Text);
    }
    /// <summary>
    /// read for element like: xyz = {str1 str2 str3}
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="name"></param>
    /// <param name="toItem"></param>
    /// <returns></returns>
    public List<TItem> ReadValues<TItem>(string name, Func<string, TItem> toItem)
    {
        return GeneralReadList<ElementScope, TItem>(name, (scope, list) =>
        {
            var items = scope.Property.Values.SelectMany(x => x.Select(x => toItem(x.Name.Text))).ToList();
            list.AddRange(items);
        });
    }

    public List<TItem> ReadValuesArray<TItem>(string name, Func<List<string>, TItem?> toItem)
    {
        return GeneralReadList<ElementArray, TItem>(name, (array, list) =>
        {
            foreach (var elements in array.Properties)
            {
                var arr = elements.Values.SelectMany(x => x.Select(x => x.Name.Text)).ToList();
                var item = toItem(arr);
                if (item is not null)
                    list.Add(item);
            }
        });
    }

    public List<KeyValuePair<TTag, TValue>> ReadTagValuesArray<TTag, TValue>(string name, Func<string, TTag> toTag, Func<List<string>, TValue> toValue)
    {
        return GeneralReadList<ElementScope, KeyValuePair<TTag, TValue>>(name, (scope, list) =>
        {
            var tag = toTag(scope.Tag.Text);
            var values = scope.Property.SelectMany(x => x.Value.Select(x => x.Name.Text)).ToList();
            list.Add(new(tag, toValue(values)));
        });
    }

    /// <summary>
    /// read for <see cref="ISsSerializable"/> object, not for array of object, otherwise use <see cref="ReadObjects"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="default"></param>
    /// <returns></returns>
    /// <exception cref="SsParseExceptions">multiAssignment of name</exception>
    public T ReadObject<T>(T @default) where T : ISsSerializable
    {
        if (!Elements.TryGetValue(@default.LocalName, out var elements) || elements.Count is 0)
            return @default;
        if (elements.Count > 1)
            throw SsParseExceptions.MultiAssignment(@default.LocalName);
        if (elements[0] is not ElementScope scope)
            throw SsParseExceptions.WrongObjectEntry(@default.LocalName);
        new SsDeserializer(@default).Deserialize(scope.Property);
        return @default;
    }

    /// <summary>
    /// read for array of <see cref="ISsSerializable"/> object, not for single object, otherwise use <see cref="ReadObject"/>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="Deserializer"></param>
    /// <param name="list"></param>
    public List<TItem> ReadObjects<TItem>() where TItem : ISsSerializable, new()
    {
        var item = new TItem();
        return GeneralReadList<ElementArray, TItem>(item.LocalName, (array, list) =>
        {
            foreach (var elements in array.Properties)
            {
                item = new();
                new SsDeserializer(item).Deserialize(elements);
                list.Add(item);
            }
        });
    }
}
