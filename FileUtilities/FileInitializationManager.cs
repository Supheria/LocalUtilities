﻿using LocalUtilities.Interface;

namespace LocalUtilities.FileUtilities;

public static class FileInitializationManager
{
    /// <summary>
    /// 根目录
    /// </summary>
    private static readonly DirectoryInfo RootDirectoryInfo = Directory.CreateDirectory("ini");

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string GetInitializationFilePath<T>(this T obj) where T : IInitializationManageable
    {
        var cachePath = Path.Combine(RootDirectoryInfo.FullName, obj.IniFileName);
        return cachePath;
    }

    /// <summary>
    /// 删除所有配置
    /// </summary>
    public static void ClearInitialization<T>(this T obj) where T : IInitializationManageable
    {
        var path = obj.GetInitializationFilePath();
        if (File.Exists(path))
            File.Delete(path);
    }
    /// <summary>
    /// 清空根目录
    /// </summary>
    public static void Clear()
    {
        Directory.Delete(RootDirectoryInfo.FullName, true);
    }
}
