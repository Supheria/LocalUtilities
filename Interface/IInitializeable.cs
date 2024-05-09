namespace LocalUtilities.Interface;

public interface IInitializeable
{
    public string LocalName { get; }

    public string? IniFileName { get; }

    public string IniFileExtension { get; }
}
