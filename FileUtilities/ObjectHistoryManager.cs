namespace LocalUtilities.FileUtilities;

public static class ObjectHistoryManager
{
    /// <summary>
    /// 较最近一次保存是否已被编辑
    /// </summary>
    public static bool IsEditThanLastSavedHistory<T>(this T obj) where T : IHistoryRecordable
    {
        return obj.LastSavedIndex != obj.CurrentHistoryIndex;
    }

    /// <summary>
    /// 判断是否有下一个历史记录
    /// </summary>
    /// <returns>是否有下一个历史记录</returns>
    public static bool HasNextHistory<T>(this T obj) where T : IHistoryRecordable
    {
        return obj.CurrentHistoryIndex + 1 < obj.CurrentHistoryLength;
    }

    /// <summary>
    /// 判断是否有上一个历史记录
    /// </summary>
    /// <returns>是否有上一个历史记录</returns>
    public static bool HasPrevHistory<T>(this T obj) where T : IHistoryRecordable
    {
        return obj.CurrentHistoryIndex > 0;
    }

    /// <summary>
    /// 将当前的状态添加到历史记录（会使后续的记录失效）
    /// </summary>
    public static void EnqueueHistory<T>(this T obj) where T : IHistoryRecordable
    {
        var data = obj.ToHashString();

        // 校验是否与上一个历史记录重复
        if (data == obj.HistoryCache[obj.CurrentHistoryIndex])
            return;
        //
        // 第一个历史记录
        //
        if (obj.CurrentHistoryLength == 0)
        {
            obj.CurrentHistoryLength++;
            obj.HistoryCache[obj.CurrentHistoryIndex] = data;
        }
        //
        // 新增的历史记录
        //
        else if (obj.CurrentHistoryLength >= obj.HistoryCache.Length) // 如果已在历史记录的结尾
        {
            // 将所有历史记录向左移动一位（不删除当前位）
            for (var i = 0; i < obj.CurrentHistoryLength - 1; i++)
            {
                obj.HistoryCache[i] = obj.HistoryCache[i + 1];
            }
            obj.HistoryCache[obj.CurrentHistoryIndex] = data;
        }
        //
        // 如果在历史记录的中间
        //
        else if (obj.CurrentHistoryIndex < obj.HistoryCache.Length - 1)
        {
            obj.CurrentHistoryIndex++; // 指向下一个地址
            obj.CurrentHistoryLength = obj.CurrentHistoryIndex + 1; // 扩展长度
            obj.HistoryCache[obj.CurrentHistoryIndex] = data;
        }
    }

    /// <summary>
    /// 撤回 (已检查 HasPrevHistory())
    /// </summary>
    public static void Undo<T>(this T obj) where T : IHistoryRecordable
    {
        if (obj.HasPrevHistory() == false)
        {
            return;
        }
        obj.CurrentHistoryIndex--;
        obj.FromHashString(obj.HistoryCache[obj.CurrentHistoryIndex]);
    }

    /// <summary>
    /// 重做 (已检查 HasNextHistory())
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">访问的历史记录越界</exception>
    public static void Redo<T>(this T obj) where T : IHistoryRecordable
    {
        if (obj.HasNextHistory() == false)
        {
            return;
        }
        obj.CurrentHistoryIndex++;
        if (obj.CurrentHistoryIndex >= obj.CurrentHistoryLength)
        {
            throw new IndexOutOfRangeException("[2302191735] 历史记录越界");
        }
        obj.FromHashString(obj.HistoryCache[obj.CurrentHistoryIndex]);
    }
    /// <summary>
    /// 清空历史记录并将当前的状态添加到历史记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public static void NewHistory<T>(this T obj) where T : IHistoryRecordable
    {
        obj.CurrentHistoryLength = 0;
        obj.CurrentHistoryIndex = 0;
        obj.LastSavedIndex = 0;
        obj.EnqueueHistory();
    }
    /// <summary>
    /// 更新最近一次的保存
    /// </summary>
    public static void UpdateLastSavedHistoryIndex<T>(this T obj) where T : IHistoryRecordable
    {
        obj.LastSavedIndex = obj.CurrentHistoryIndex;
    }
}