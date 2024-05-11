using LocalUtilities.FileUtilities;
using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Parser;
using System.Data.SqlTypes;

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
            message = null;
            foreach (var token in new Tokenizer(path).Tokens)
            {
                if (serialization.Deserialize(token))
                    return serialization.Source;
            }
            throw new SsParseExceptions($"cannot find an entry of {serialization.LocalName}");
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }
        return serialization.Source;
    }
}
