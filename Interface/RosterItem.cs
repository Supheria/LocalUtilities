namespace LocalUtilities.Interface;

public abstract class RosterItem<TSignature>(TSignature signature) where TSignature : notnull
{
    public TSignature Signature { get; set; } = signature;
}
