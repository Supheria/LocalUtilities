namespace LocalUtilities.SimpleScript;

public abstract class SignTable
{
    public abstract char Empty { get; }
    public abstract char Note { get; }
    public abstract char Quote { get; }
    public abstract char Escape { get; }
    public abstract char Open { get; }
    public abstract char Close { get; }
    public abstract char Equal { get; }
    public abstract char Greater { get; }
    public abstract char Less { get; }
    public abstract char Tab { get; }
    public abstract char Space { get; }
    public abstract char Return { get; }
    public abstract char NewLine { get; }
}
