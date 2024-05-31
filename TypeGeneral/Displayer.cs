

namespace LocalUtilities.TypeGeneral;

public abstract class Displayer : PictureBox
{
    protected Directions DragFlag { get; set; } = Directions.None;

    protected Point DragStartPoint { get; set; }

    protected virtual FontData LabelFontData { get; set; } = new(nameof(LabelFontData))
    {
        Size = 17f,
    };

    protected virtual FontData ContentFontData { get; set; } = new(nameof(ContentFontData));

    public Displayer()
    {
        MouseDown += OnMouseDown;
        AddOperations();
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        DragFlag = e.Button switch
        {
            MouseButtons.Left => Directions.Left,
            MouseButtons.Right => Directions.Right,
            MouseButtons.Middle => Directions.Center,
            _ => Directions.None,
        };
        DragStartPoint = e.Location;
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        DragFlag = Directions.None;
    }

    protected abstract void AddOperations();

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        DragStartPoint = e.Location;
    }

    protected void Relocate()
    {
        if (Size == Image?.Size || Size.Width is 0 || Size.Height is 0)
            return;
        Image?.Dispose();
        Image = new Bitmap(Width, Height);
    }
}
