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

    /// <summary>
    /// write for all items in collection of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="collection"></param>
    /// <param name="itemSerialization"></param>
    public void Serialize<T>(ICollection<T> collection) where T : ISsSerializable
    {
        foreach (var item in collection)
            new SsSerializer(item, Writer).Serialize();
    }
}
