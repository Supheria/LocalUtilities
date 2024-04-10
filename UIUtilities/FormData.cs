namespace LocalUtilities.UIUtilities;

public class FormData
{
    public Size Size { get; set; } = new();

    public Point Location { get; set; } = new();

    public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
}
