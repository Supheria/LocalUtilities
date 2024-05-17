using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public delegate void FormOnRunninigDelegate();

public abstract class ResizeableForm<TFormData> : Form where TFormData : FormData, new()
{
    bool Resizing { get; set; } = false;

    protected FormOnRunninigDelegate? OnLoadFormData { get; set; }

    protected FormOnRunninigDelegate? OnSaveFormData { get; set; }

    protected FormOnRunninigDelegate? OnDrawingClient { get; set; }

    protected TFormData FormData { get; set; } = new();

    protected new int Left => ClientRectangle.Left;

    protected new int Top => ClientRectangle.Top;

    protected new int Width => ClientRectangle.Width;

    protected new int Height => ClientRectangle.Height;

    public ResizeableForm()
    {
        ResizeBegin += ResizeableForm_ResizeBegin;
        ResizeEnd += ResizeableForm_ResizeEnd;
        SizeChanged += ResizeableForm_SizeChanged;
        Load += ResizeableForm_Load;
        FormClosing += ResizeableForm_FormClosing;
        InitializeComponent();
    }

    private void ResizeableForm_ResizeBegin(object? sender, EventArgs e)
    {
        Resizing = true;
    }

    private void ResizeableForm_ResizeEnd(object? sender, EventArgs e)
    {
        Resizing = false;
        DrawClient();
    }

    private void ResizeableForm_SizeChanged(object? sender, EventArgs e)
    {
        if (Resizing is false)
            DrawClient();
    }

    private void ResizeableForm_Load(object? sender, EventArgs e)
    {
        FormData = FormData.LoadFromSimpleScript();
        OnLoadFormData?.Invoke();
        MinimumSize = FormData.MinimumSize;
        Size = FormData.Size;
        Location = FormData.Location;
        WindowState = FormData.WindowState;
        DrawClient();
    }

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        OnSaveFormData?.Invoke();
        FormData.MinimumSize = MinimumSize;
        FormData.Size = Size;
        FormData.Location = Location;
        FormData.WindowState = WindowState;
        FormData.SaveToSimpleScript(true);
    }

    protected abstract void InitializeComponent();

    private void DrawClient()
    {
        if (WindowState is FormWindowState.Minimized)
            return;
        SuspendLayout();
        OnDrawingClient?.Invoke();
        ResumeLayout();
    }
}
