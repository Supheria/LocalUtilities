using LocalUtilities.FileUtilities;
using LocalUtilities.Interface;

namespace LocalUtilities.UIUtilities;

public abstract class ResizeableForm<TFormData> : Form where TFormData : FormData
{
    bool _resizing { get; set; } = false;

    protected FormDataLoadDelegate? OnLoadFormData { get; set; }

    protected FormDataSaveDelegate? OnSaveFormData { get; set; }

    protected TFormData FormData { get; set; }

    FormDataXmlSerialization<TFormData> FormDataXmlSerialization { get; }


    public ResizeableForm(TFormData formData, FormDataXmlSerialization<TFormData> formDataXmlSerialization)
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

    private void ResizeableForm_ResizeBegin(object? sender, EventArgs e) => _resizing = true;

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
        FormData = FormDataXmlSerialization.LoadFromXml(out _);
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
        FormDataXmlSerialization.Source = FormData;
        FormDataXmlSerialization.SaveToXml();
    }

    protected abstract void InitializeComponent();

    protected abstract void DrawClient();
}
