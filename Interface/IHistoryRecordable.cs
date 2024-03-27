using LocalUtilities.SerializeUtilities;

namespace LocalUtilities.Interface;

public interface IHistoryRecordable : IHashStringCheckable
{
    /// <summary>
    /// 历史记录指针
    /// </summary>
    int HistoryIndex { get; set; }
    /// <summary>
    /// 当前历史记录长度
    /// </summary>
    int CurrentHistoryLength { get; set; }
    /// <summary>
    /// 开辟的的历史记录保存空间
    /// </summary>
    string[] History { get; set; }
    /// <summary>
    /// 最近一次保存时所在的历史记录指针
    /// </summary>
    int LatestIndex { get; set; }
}