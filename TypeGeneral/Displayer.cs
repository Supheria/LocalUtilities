

namespace LocalUtilities.TypeGeneral;

public abstract class Displayer : PictureBox
{
    public virtual Color FrontColor { get; set; }

    public virtual new Size Padding { get; set; }

    protected Rectangle ClientRect => ClientRectangle;

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

    protected virtual FontData LabelFontData { get; set; } = new()
    {
        Size = 17f,
    };

    protected virtual FontData ContentFontData { get; set; } = new();

    public Displayer()
    {
        AddOperation();
    }

    protected virtual void AddOperation()
    {

    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Redraw();
    }

    public virtual void Redraw()
    {
        if (ClientSize == Image?.Size)
            return;
        Image?.Dispose();
        var width = ClientWidth > 0 ? ClientWidth : 1;
        var height = ClientHeight > 0 ? ClientHeight : 1;
        Image = new Bitmap(width, height);
    }

    public virtual void EnableListener()
    {

    }

    public virtual void DisableListener()
    {

    }
}
