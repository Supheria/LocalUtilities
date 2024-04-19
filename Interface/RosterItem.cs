namespace LocalUtilities.Interface;

public abstract class RosterItem<TSignature>(TSignature signature) where TSignature : notnull
{
    public TSignature Signature { get; protected set; } = signature;

    public TSignature SetSignature { set => Signature = value; }
}
