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
        Writer.AppendTag(name, tag);
    }

    public void WriteValue<TItem>(string name, TItem item, Func<TItem, List<string>> toStrings)
    {
        Writer.AppendValues(name, toStrings(item));
    }

    public void WriteValues<TItem>(string name, List<TItem> items, Func<TItem, string> toString)
    {
        Writer.AppendValues(name, items.Select(x => toString(x)).ToList());
    }

    public void WriteTagValues<TItem>(string name, string tag, List<TItem> items, Func<TItem, string> toString)
    {
        Writer.AppendTagValues(name, tag, items.Select(x => toString(x)).ToList());
    }

    public void WriteValueArrays<TItem>(string name, List<TItem> items, Func<TItem, List<string>> toStringArray)
    {
        Writer.AppendValueArrays(name, items.Select(x => toStringArray(x)).ToList());
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
    public void WriteObjects<T>(ICollection<T> array) where T : ISsSerializable, new()
    {
        Writer.AppendNameStart(new T().LocalName);
        foreach(var obj in array)
        {
            Writer.AppendArrayStart();
            obj.Serialize(this);
            Writer.AppendArrayEnd();
        }
        Writer.AppendNameEnd();
    }
}
