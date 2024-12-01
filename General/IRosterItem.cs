namespace LocalUtilities.General;

public interface IRosterItem<TSignature> where TSignature : notnull
{
    public TSignature Signature { get; }
}
