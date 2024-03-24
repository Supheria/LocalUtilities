namespace LocalUtilities.SerializeUtilities;

public interface IFormattedObject
{
    /// <summary>
    /// 获取格式化数据
    /// </summary>
    /// <returns>格式化数据</returns>
    public FormattedData ToFormattedData();
    /// <summary>
    /// 复原格式化数据
    /// </summary>
    /// <param name="data">格式化数据</param>
    public void FromFormattedData(FormattedData data);
}