namespace LocalUtilities.TypeGeneral;

public interface IRosterItem<TSignature> where TSignature : notnull
{
    public TSignature Signature { get; }
}
