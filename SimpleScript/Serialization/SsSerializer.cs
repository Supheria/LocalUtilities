using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

    /// <summary>
    /// write for property of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="property"></param>
    /// <param name="serialization"></param>
    public void Serialize<T>(T property) where T : ISsSerializable
    {
        new SsSerializer(property, Writer).Serialize();
    }

    public void WriteComment(string comment)
    {
        Writer.AppendComment(comment);
    }

    /// <summary>
    /// write for a pure token
    /// </summary>
    /// <param name="name"></param>
    public void WriteToken(string name)
    {
        Writer.AppendToken(name);
    }

    /// <summary>
    /// write for a tag of given name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tag"></param>
    public void WriteTag(string name, string tag)
    {
        Writer.AppendTagValue(name, tag, []);
    }

    public void WriteValue<TItem>(string name, TItem item, Func<TItem, List<string>> toStrings)
    {
        Writer.AppendValue(name, toStrings(item));
    }

    public void WriteTagValue<TItem>(string name, string tag, List<TItem> items, Func<TItem, string> toString)
    {
        Writer.AppendTagValue(name, tag, items.Select(x => toString(x)).ToList());
    }

    public void WriteValueArrays<TItem>(string name, List<TItem> items, Func<TItem, List<string>> toStringArray)
    {
        Writer.AppendValueArrays(name, items.Select(x => toStringArray(x)).ToList());
    }

    /// <summary>
    /// write for all items in collection of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="collection"></param>
    /// <param name="itemSerialization"></param>
    public void WriteSerializableItems<T>(ICollection<T> collection) where T : ISsSerializable
    {
        foreach (var item in collection)
            new SsSerializer(item, Writer).Serialize();
    }
}
