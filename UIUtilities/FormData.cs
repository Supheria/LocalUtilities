namespace LocalUtilities.UIUtilities;

public abstract class FormData
{
    public abstract Size MinimumSize { get; set; }

    public virtual Size Size { get; set; }

    public virtual Point Location { get; set; }

    public virtual FormWindowState WindowState { get; set; } = FormWindowState.Normal;
}
