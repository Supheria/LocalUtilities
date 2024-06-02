namespace LocalUtilities.FileHelper;

public static class InitializeManager
{
    private static readonly DirectoryInfo RootDirectoryInfo = Directory.CreateDirectory("ini");


    public static string GetInitializeFilePath<T>(this T obj) where T : IInitializeable
    {
        var cachePath = Path.Combine(RootDirectoryInfo.FullName, obj.InitializeName);
        return Path.ChangeExtension(cachePath, obj.IniFileExtension);
    }

    public static void DeleteInitializeFile<T>(this T obj) where T : IInitializeable
    {
        var path = GetInitializeFilePath(obj);
        if (File.Exists(path))
            File.Delete(path);
    }

    public static void DeleteRoot()
    {
        Directory.Delete(RootDirectoryInfo.FullName, true);
    }
}
