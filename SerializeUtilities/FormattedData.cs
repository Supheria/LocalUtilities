namespace LocalUtilities.SerializeUtilities;

public class FormattedData
{
    public string[] Items { get; }

    public FormattedData(params string[] data) => Items = data;
}