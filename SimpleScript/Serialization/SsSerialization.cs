using LocalUtilities.FileUtilities;
using LocalUtilities.Interface;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using System.Text;
using System.Xml.Linq;

namespace LocalUtilities.SimpleScript.Serialization;

public delegate void SerializationOnRunning();

public abstract class SsSerialization<T>() : IInitializeable where T : new()
{
    public T Source = new();

    public abstract string LocalName { get; }

    public string IniFileName => LocalName;

    public string IniFileExtension { get; } = "ss";

    protected SerializationOnRunning? OnSerialize { get; set; } = null;

    protected SerializationOnRunning? OnDeserialize { get; set; } = null;

    SsSerializer? Serializer { get; set; }

    Scope? Scope { get; set; } = null;


    ////
    ////
    ////
    
    
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

    public T LoadFromFile(out string? message, string? inFilePath = null)
    {
        var path = inFilePath ?? this.GetInitializationFilePath();
        if (!File.Exists(path))
        {
            message = $"\"{path}\" file path is not existed.";
            return Source;
        }
        try
        {
            message = null;
            foreach (var token in new Tokenizer(path).Tokens)
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


    ////
    ////
    ////


    public void Serialize(SsSerializer serializer)
    {
        serializer.AppendNameStart(LocalName);
        Serializer = serializer;
        OnSerialize?.Invoke();
        serializer.AppendNameEnd();
    }

    protected void WriteToken(string name)
    {
        if (Serializer is null)
            return;
        Serializer.AppendToken(name);
    }

    protected void WriteTag(string name, string property)
    {
        if (Serializer is null)
            return;
        Serializer.AppendTag(name, property);
    }

    protected void Serialize<TProperty>(TProperty property, SsSerialization<TProperty> serialization) where TProperty : new()
    {
        if (Serializer is null)
            return;
        serialization.Source = property;
        serialization.Serialize(Serializer);
    }

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


    ////
    ////
    ////


    public bool Deserialize(Token token)
    {
        Source = new();
        if (token.Name.Text == LocalName && token is Scope scope)
        {
            Scope = scope;
            OnDeserialize?.Invoke();
            return true;
        }
        return false;
    }

    protected TProperty ReadTag<TProperty>(string name, Func<string?, TProperty> toT)
    {
        string? str = null;
        if (Scope is null || !Scope.Property.TryGetValue(name, out var tokens) || tokens.Count is 0)
            return toT(str); 
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
        return toT(str);
    }

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

    protected void Deserialize(string name, Action<Token> addToken)
    {
        if (Scope is null || !Scope.Property.TryGetValue(name, out var tokens) || tokens.Count is 0)
            return;
        foreach (var token in tokens)
            addToken(token);
    }

    protected void Deserialize(Type type, Action<Token> addToken)
    {
        if (Scope is null)
            return;
        foreach (var tokens in Scope.Property.Values)
        {
            foreach(var token in tokens)
            {
                if (token.GetType() == type)
                    addToken(token);
            }
        }
    }
}
