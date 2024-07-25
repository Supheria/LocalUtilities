namespace LocalUtilities.SimpleScript;

public class DataName(string name)
{
    public string Name { get; } = name;

    /// <summary>
    /// set default <see cref="Name"/> value to "Ss"
    /// </summary>
    public DataName() : this("Ss")
    {

    }
}
