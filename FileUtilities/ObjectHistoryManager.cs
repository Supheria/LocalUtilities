using LocalUtilities.Interface;

namespace LocalUtilities.FileUtilities;

public static class ObjectHistoryManager
{
    /// <summary>
    /// 较最近一次保存是否已被编辑
    /// </summary>
    public static bool IsEdit<T>(this T obj) where T : IHistoryRecordable => obj.LatestIndex != obj.HistoryIndex;

    /// <summary>
    /// 判断是否有下一个历史记录
    /// </summary>
    /// <returns>是否有下一个历史记录</returns>
    public static bool HasNextHistory<T>(this T obj) where T : IHistoryRecordable => obj.HistoryIndex + 1 < obj.CurrentHistoryLength;

    /// <summary>
    /// 判断是否有上一个历史记录
    /// </summary>
    /// <returns>是否有上一个历史记录</returns>
    public static bool HasPrevHistory<T>(this T obj) where T : IHistoryRecordable => obj.HistoryIndex > 0;

    /// <summary>
    /// 将当前的状态添加到历史记录（会使后续的记录失效）
    /// </summary>
    public static void EnqueueHistory<T>(this T obj) where T : IHistoryRecordable
    {
        var data = obj.ToHashString();

        // 校验是否与上一个历史记录重复
        if (data == obj.History[obj.HistoryIndex])
            return;
        //
        // 第一个历史记录
        //
        if (obj.CurrentHistoryLength == 0)
        {
            obj.CurrentHistoryLength++;
            obj.History[obj.HistoryIndex] = data;
        }
        //
        // 新增的历史记录
        //
        else if (obj.CurrentHistoryLength >= obj.History.Length) // 如果已在历史记录的结尾
        {
            // 将所有历史记录向左移动一位（不删除当前位）
            for (var i = 0; i < obj.CurrentHistoryLength - 1; i++)
            {
                obj.History[i] = obj.History[i + 1];
            }
            obj.History[obj.HistoryIndex] = data;
        }
        //
        // 如果在历史记录的中间
        //
        else if (obj.HistoryIndex < obj.History.Length - 1)
        {
            obj.HistoryIndex++; // 指向下一个地址
            obj.CurrentHistoryLength = obj.HistoryIndex + 1; // 扩展长度
            obj.History[obj.HistoryIndex] = data;
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
        obj.HistoryIndex--;
        obj.FromHashString(obj.History[obj.HistoryIndex]);
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
        obj.HistoryIndex++;
        if (obj.HistoryIndex >= obj.CurrentHistoryLength)
        {
            throw new IndexOutOfRangeException("[2302191735] 历史记录越界");
        }
        obj.FromHashString(obj.History[obj.HistoryIndex]);
    }
    /// <summary>
    /// 清空历史记录并将当前的状态添加到历史记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public static void NewHistory<T>(this T obj) where T : IHistoryRecordable
    {
        obj.CurrentHistoryLength = 0;
        obj.HistoryIndex = 0;
        obj.LatestIndex = 0;
        obj.EnqueueHistory();
    }
    /// <summary>
    /// 更新最近一次的保存
    /// </summary>
    public static void UpdateLatest<T>(this T obj) where T : IHistoryRecordable => obj.LatestIndex = obj.HistoryIndex;
}