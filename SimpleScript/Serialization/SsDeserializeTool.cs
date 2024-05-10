using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SsDeserializeTool
{
    public static T LoadFromFile<T>(this SsSerialization<T> serialization, out string? message)
    {
        var path = serialization.GetInitializationFilePath();
        if (!File.Exists(path))
        {
            message = $"\"{path}\" file path is not existed.";
            return serialization.Source;
        }
        try
        {
            var tokenizer = new Tokenizer(path);
            tokenizer.ParseToNextStep();
            var token = tokenizer.Tokens.FirstOrDefault(t => t.Name == serialization.LocalName);
            serialization.DoDeserialize(token ?? throw new ArgumentException());
            message = null;
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return serialization.Source;
    }

    public static T Deserialize<T>(this SsSerialization<T> serialization, Token token)
    {
        serialization.DoDeserialize(token);
        return serialization.Source;
    }
}
