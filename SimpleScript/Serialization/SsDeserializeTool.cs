using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;

namespace LocalUtilities.SimpleScript.Serialization;

public static class SsDeserializeTool
{
    public static T LoadFromFile<T>(this SsSerialization<T> serialization, out string message)
    {
        var path = serialization.GetInitializationFilePath();
        if (!File.Exists(path))
            message = $"\"{path}\" file path is not existed.";
        else
        {
            try
            {
                var exceptions = new Exceptions();
                var token = new Tokenizer(exceptions, path).Tokens.FirstOrDefault(t => t.Name == serialization.LocalName);
                serialization.Deserialize(token ?? throw new ArgumentException());
                message = exceptions.Log.ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }
        return serialization.Source;
    }

    public static T Deserialize<T>(this SsSerialization<T> serialization, Token token)
    {
        serialization.Deserialize(token);
        return serialization.Source;
    }
}
