using LocalUtilities.FileUtilities;
using System.Text;

namespace LocalUtilities.SimpleScript.Serialization;

partial class SsSerialization<T>
{
    public override string ToString()
    {
        var serializer = new SsSerializer(false);
        Serialize(serializer);
        return serializer.ToString();
    }

    public string? SaveToFile(bool writeIntoMultiLines, string? outFilePath = null)
    {
        string? message = null;
        try
        {
            var path = outFilePath ?? this.GetInitializationFilePath();
            using var file = File.Create(path);
            var serializer = new SsSerializer(writeIntoMultiLines);
            Serialize(serializer);
            file.Write([0xEF, 0xBB, 0xBF]);
            using var streamWriter = new StreamWriter(file, Encoding.UTF8);
            streamWriter.Write(serializer.ToString());
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return message;
    }

    /// <summary>
    /// write begin of this
    /// </summary>
    /// <param name="serializer"></param>
    private void Serialize(SsSerializer serializer)
    {
        serializer.AppendNameStart(LocalName);
        Serializer = serializer;
        Serialize();
        serializer.AppendNameEnd();
    }

    /// <summary>
    /// write begin of successor
    /// </summary>
    protected abstract void Serialize();

    /// <summary>
    /// write for a pure token
    /// </summary>
    /// <param name="name"></param>
    protected void WriteToken(string name)
    {
        if (Serializer is null)
            return;
        Serializer.AppendToken(name);
    }

    /// <summary>
    /// write for a tag of given name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tag"></param>
    protected void WriteTag(string name, string tag)
    {
        if (Serializer is null)
            return;
        Serializer.AppendTag(name, tag);
    }

    /// <summary>
    /// write for property of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="property"></param>
    /// <param name="serialization"></param>
    protected void Serialize<TProperty>(TProperty property, SsSerialization<TProperty> serialization) where TProperty : new()
    {
        if (Serializer is null)
            return;
        serialization.Source = property;
        serialization.Serialize(Serializer);
    }

    /// <summary>
    /// write for all items in collection of given type
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="collection"></param>
    /// <param name="itemSerialization"></param>
    public void Serialize<TProperty>(ICollection<TProperty> collection, SsSerialization<TProperty> itemSerialization) where TProperty : new()
    {
        if (Serializer is null)
            return;
        foreach (var item in collection)
        {
            itemSerialization.Source = item;
            itemSerialization.Serialize(Serializer);
        }
    }
}
