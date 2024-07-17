namespace LocalUtilities.TypeGeneral;

public abstract class Displayer : PictureBox
{
    protected virtual FontData LabelFontData { get; set; } = new()
    {
        Size = 17f,
    };

    protected virtual FontData ContentFontData { get; set; } = new();

    public void Relocate()
    {
        if (Size == Image?.Size || Size.Width is 0 || Size.Height is 0)
            return;
        Image?.Dispose();
        Image = new Bitmap(Width, Height);
    }
}
