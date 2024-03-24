namespace LocalUtilities.ManageUtilities;

public interface IFileBackupManageable : IFileManageable
{
    /// <summary>
    /// 获取对象哈希值字符串
    /// </summary>
    /// <returns></returns>
    string GetHashString();

    /// <summary>
    /// 从文件路径获取新的对象的哈希值字符串
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    string GetHashStringFromFilePath(string filePath);
}