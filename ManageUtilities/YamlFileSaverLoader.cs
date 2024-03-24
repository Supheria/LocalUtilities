using YamlDotNet.Serialization;

namespace LocalUtilities.ManageUtilities;

public static class YamlFileSaverLoader
{
    public static void SaveToYaml<T>(this T obj, string path)
    {
        var streamWriter = File.CreateText(path);
        var yamlSerializer = new Serializer();
        yamlSerializer.Serialize(streamWriter, obj);
        streamWriter.Close();
    }

    public static string LoadFromYaml<T>(string path, out T? obj)
    {
        obj = default;
        if (!File.Exists(path))
            return $"{path} is not existed.";
        var streamReader = File.OpenText(path);
        try
        {
            var yamlDeserializer = new Deserializer();
            obj = yamlDeserializer.Deserialize<T>(streamReader);
            streamReader.Close();
            return "";
        }
        catch (Exception e)
        {
            streamReader.Close();
            return e.Message;
        }
    }
}