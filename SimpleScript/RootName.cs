namespace LocalUtilities.SimpleScript;

public class RootName(string? name)
{
    public string? Name { get; } = name;

    /// <summary>
    /// set default <see cref="Name"/> value to "Ss"
    /// </summary>
    public RootName() : this(null)
    {

    }
}
