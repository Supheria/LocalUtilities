namespace LocalUtilities.UIUtilities;

public abstract class FormData
{
    public abstract Size MinimumSize { get; set; }

    public virtual Size Size { get; set; }

    public virtual Point Location { get; set; }

    public virtual FormWindowState WindowState { get; set; } = FormWindowState.Normal;

    public virtual int Padding { get; set; } = 12;

    public virtual FontData LabelFontData { get; set; } = new("黑体", 0.03f, FontStyle.Regular, GraphicsUnit.Pixel);

    public virtual FontData ContentFontData { get; set; } = new("黑体", 0.05f, FontStyle.Regular, GraphicsUnit.Pixel);
}
