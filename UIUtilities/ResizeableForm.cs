using LocalUtilities.DelegateUtilities;
using LocalUtilities.FileUtilities;
using LocalUtilities.Serializations;
using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.UIUtilities;

public abstract class ResizeableForm<TFormData> : Form where TFormData : FormData
{
    bool _resizing { get; set; } = false;

    protected FormOnRunninigDelegate? OnLoadFormData { get; set; }

    protected FormOnRunninigDelegate? OnSaveFormData { get; set; }

    protected FormOnRunninigDelegate? OnDrawingClient { get; set; }

    protected TFormData FormData { get; set; }

    protected new int Padding { get; set; }

    FormDataSerialization<TFormData> FormDataXmlSerialization { get; }

    protected new int Left => ClientRectangle.Left;

    protected new int Top => ClientRectangle.Top;

    protected new int Width => ClientRectangle.Width;

    protected new int Height => ClientRectangle.Height;

    public ResizeableForm(TFormData formData, FormDataSerialization<TFormData> formDataXmlSerialization)
    {
        FormData = formData;
        FormDataXmlSerialization = formDataXmlSerialization;
        ResizeBegin += ResizeableForm_ResizeBegin;
        ResizeEnd += ResizeableForm_ResizeEnd;
        SizeChanged += ResizeableForm_SizeChanged;
        Load += ResizeableForm_Load;
        FormClosing += ResizeableForm_FormClosing;
        InitializeComponent();
    }

    private void ResizeableForm_ResizeBegin(object? sender, EventArgs e)
    {
        _resizing = true;
    }

    private void ResizeableForm_ResizeEnd(object? sender, EventArgs e)
    {
        _resizing = false;
        DrawClient();
    }

    private void ResizeableForm_SizeChanged(object? sender, EventArgs e)
    {
        if (_resizing is false)
            DrawClient();
    }

    private void ResizeableForm_Load(object? sender, EventArgs e)
    {
        FormData = FormDataXmlSerialization.LoadFromFile(out _);
        OnLoadFormData?.Invoke();
        MinimumSize = FormData.MinimumSize;
        Size = FormData.Size;
        Location = FormData.Location;
        WindowState = FormData.WindowState;
        Padding = FormData.Padding;
        DrawClient();
    }

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        OnSaveFormData?.Invoke();
        FormData.MinimumSize = MinimumSize;
        FormData.Size = Size;
        FormData.Location = Location;
        FormData.WindowState = WindowState;
        FormData.Padding = Padding;
        FormDataXmlSerialization.Source = FormData;
        FormDataXmlSerialization.SaveToFile(true);
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
