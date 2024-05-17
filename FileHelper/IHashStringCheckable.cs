namespace LocalUtilities.FileHelper;

public interface IHashStringCheckable : IFileManageable
{
    string HashCachePath { get; }
    /// <summary>
    /// 获取对象哈希值字符串
    /// </summary>
    /// <returns></returns>
    string ToHashString();
    /// <summary>
    /// 从文件路径获取新的对象的哈希值字符串
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    string ToHashString(string filePath);
    /// <summary>
    /// 从哈希值字符串还原对象
    /// </summary>
    /// <param name="data">格式化数据</param>
    void FromHashString(string data);
}