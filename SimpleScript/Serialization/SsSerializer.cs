namespace LocalUtilities.SimpleScript.Serialization;

public class SsSerializer(object obj, SsWriter writer) : SsSerializeBase(obj)
{
    SsWriter Writer { get; } = writer;

    /// <summary>
    /// write begin of this
    /// </summary>
    /// <param name="serializer"></param>
    public string Serialize()
    {
        Writer.AppendNameStart(Source.LocalName);
        Source.Serialize(this);
        Writer.AppendNameEnd();
        return Writer.ToString();
    }

    public void WriteComment(string comment)
    {
        Writer.AppendComment(comment);
    }

    /// <summary>
    /// write for a tag of given name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tag"></param>
    public void WriteTag(string name, string tag)
    {
        Writer.AppendTag(name, tag);
    }

    public void WriteValues<TItem>(string name, List<TItem> items, Func<TItem, string> toString)
    {
        Writer.AppendValues(name, items.Select(x => toString(x)).ToList());
    }

    public void WriteValuesArray<TItem>(string name, List<TItem> items, Func<TItem, List<string>> toValues)
    {
        var array = new List<List<string>>();
        foreach (var item in items)
            array.Add(toValues(item));
        Writer.AppendValuesArray(name, array);
    }

    public void WriteTagValuesArray<TTag, TValue>(string name, ICollection<KeyValuePair<TTag, TValue>> pairs, Func<TTag, string> writeKey, Func<TValue, List<string>> writeValue)
    {
        foreach (var (tag, value) in pairs)
            Writer.AppendTagValues(name, writeKey(tag), writeValue(value));
    }

    /// <summary>
    /// write for <see cref="ISsSerializable"/> object, not for array of object, otherwise use <see cref="WriteObjects"/>
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="property"></param>
    /// <param name="serialization"></param>
    public void WriteObject<T>(T property) where T : ISsSerializable
    {
        new SsSerializer(property, Writer).Serialize();
    }

    /// <summary>
    /// write for array of <see cref="ISsSerializable"/> object, not for single object, otherwise use <see cref="WriteObject"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public void WriteObjects<T>(string arrayName, ICollection<T> array) where T : ISsSerializable, new()
    {
        Writer.AppendNameStart(arrayName);
        foreach (var obj in array)
        {
            Writer.AppendArrayStart(null);
            obj.Serialize(this);
            Writer.AppendArrayEnd();
        }
        Writer.AppendNameEnd();
    }
}
