namespace LocalUtilities.TypeGeneral;

public abstract class RosterItem<TSignature> where TSignature : notnull
{
    public abstract TSignature Signature { get; }
}
