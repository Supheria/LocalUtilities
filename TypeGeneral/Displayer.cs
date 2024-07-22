

namespace LocalUtilities.TypeGeneral;

public abstract class Displayer : PictureBox
{
    public Color FrontColor { get; set; }

    public virtual new Size Padding { get; set; }

    protected virtual FontData LabelFontData { get; set; } = new()
    {
        Size = 17f,
    };

    protected virtual FontData ContentFontData { get; set; } = new();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Size == Image?.Size || Size.Width is 0 || Size.Height is 0)
            return;
        Image?.Dispose();
        Image = new Bitmap(Width, Height);
        Redraw();
    }

    public virtual void Redraw()
    {

    }

    //protected override void OnResize(EventArgs e)
    //{
    //    base.OnResize(e);
    //    if (Size == Image?.Size || Size.Width is 0 || Size.Height is 0)
    //        return;
    //    Image?.Dispose();
    //    Image = new Bitmap(Width, Height);
    //}
}
