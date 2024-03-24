using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace LocalUtilities.ManageUtilities;

public static partial class FileBackupManager
{
    /// <summary>
    /// 根目录
    /// </summary>
    private static readonly DirectoryInfo RootDirectoryInfo = Directory.CreateDirectory("backup");

    /// <summary>
    /// 根目录名称
    /// </summary>
    public static string RootDirectoryName => RootDirectoryInfo.FullName;


    [GeneratedRegex("^BK(\\d{4})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})$")]
    private static partial Regex BackupRegex();

    /// <summary>
    /// 对象根目录
    /// </summary>
    private static string DirectoryName<T>(this T obj) where T : IFileBackupManageable
    {
        var dir = Path.Combine(RootDirectoryName, obj.FileManageDirName);
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// 对象的文件备份路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>root\obj file manage\obj hash\date time</returns>
    private static string GetBackupFilePath<T>(this T obj) where T : IFileBackupManageable =>
        Path.Combine(obj.DirectoryName(), obj.GetHashString(), $"BK{DateTime.Now:yyyyMMddHHmmss}");

    /// <summary>
    /// 从文件路径获取备份路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="path">要获取备份路径的文件路径</param>
    /// <returns></returns>
    private static string GetBackupFilePath<T>(this T obj, string path) where T : IFileBackupManageable =>
        Path.Combine(obj.DirectoryName(), obj.GetHashStringFromFilePath(path), $"BK{DateTime.Now:yyyyMMddHHmmss}");

    /// <summary>
    /// 备份指定文件路径的obj（如果存在的话）,如果已存在备份文件则不作替换
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path">要备份的文件路径</param>
    public static void Backup<T>(this T obj, string path) where T : IFileBackupManageable
    {
        try
        {
            var backupFilePath = obj.GetBackupFilePath(path);
            var backupDir = Path.GetDirectoryName(backupFilePath);
            if (backupDir is null || Directory.Exists(backupDir))
                return;
            Directory.CreateDirectory(backupDir);
            File.Copy(path, backupFilePath, true);
        }
        catch (Exception ex)
        {
            throw new($"[2303051407]无法备份{path}。\n{ex.Message}");
        }
    }

    /// <summary>
    /// 通过文件路径的文件名查找同名文件夹下的所有备份，形成文件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path">要查找的文件路径</param>
    /// <returns>文件列表(备份文件路径, 备份名)，列表第一元素是文件路径本身</returns>
    public static List<(string, string)> GetBackupsList<T>(this T obj, string path) where T : IFileBackupManageable
    {
        List<(string, string)> result = new();
        var objManageDir = Path.GetDirectoryName(obj.IsBackupFile(path)
            ? Path.GetDirectoryName(path)
            : Path.GetDirectoryName(obj.GetBackupFilePath()));
        if (!Directory.Exists(objManageDir))
            return result;
        var backupDirs = new DirectoryInfo(objManageDir).GetDirectories();
        Array.Sort(backupDirs, (x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
        result.Add((path, Path.GetFileNameWithoutExtension(path)));
        var objBackupDir = Path.GetDirectoryName(obj.GetBackupFilePath(path));
        foreach (var backupDir in backupDirs)
        {
            var backupFile = backupDir.GetFiles().FirstOrDefault();
            if (backupFile is null)
            {
                backupDir.Delete(true);
                continue;
            }
            var backupFilePath = backupFile.FullName;
            var backupDirForTest = Path.GetDirectoryName(obj.GetBackupFilePath(backupFilePath));
            if (backupDirForTest != backupDir.FullName)
            {
                backupDir.Delete(true);
                continue;
            }
            if (backupDirForTest == objBackupDir)
                continue;

            var match = BackupRegex().Match(Path.GetFileName(backupFilePath));
            result.Add((backupFilePath,
                $"{match.Groups[1].Value}/{match.Groups[2].Value}/{match.Groups[3].Value} {match.Groups[4].Value}:{match.Groups[5].Value}:{match.Groups[6].Value}"));
        }
        return result;
    }

    /// <summary>
    /// 删除当前备份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">要删除的备份对象</param>
    public static void DeleteBackup<T>(this T obj) where T : IFileBackupManageable
    {
        var objRootDir = Path.GetDirectoryName(obj.GetBackupFilePath());
        if (Directory.Exists(objRootDir))
            Directory.Delete(objRootDir, true);
    }

    /// <summary>
    /// 清空根目录并打成压缩包
    /// </summary>
    public static void Clear(string zipPath)
    {
        ZipFile.CreateFromDirectory(RootDirectoryName, zipPath);
        Directory.Delete(RootDirectoryName, true);
        Directory.CreateDirectory(RootDirectoryName);

        Process p = new();
        p.StartInfo.FileName = "explorer.exe";
        p.StartInfo.Arguments = @" /select, " + zipPath;
        p.Start();
    }

    /// <summary>
    /// 查询文件是否是备份文件：位于备份子根目录下且具有备份文件名格式
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path">要查询的文件路径</param>
    /// <returns></returns>
    public static bool IsBackupFile<T>(this T obj, string path) where T : IFileBackupManageable
    {
        var match = BackupRegex().Match(Path.GetFileName(path));
        return match.Success && obj.DirectoryName() == Path.GetDirectoryName(Path.GetDirectoryName(path));
    }
}