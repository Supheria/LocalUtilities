using LocalUtilities.FileUtilities;
using LocalUtilities.Interface;

namespace LocalUtilities.UIUtilities;

public abstract class ResizeableForm : Form, IInitializeable
{
    public string IniFileName { get; }

    bool _resizing { get; set; } = false;

    protected FormDataLoadDelegate? OnLoadFormData { get; }

    protected FormDataSaveDelegate? OnSaveFormData { get; }


    public ResizeableForm(string iniFileName)
    {
        IniFileName = iniFileName;
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
        var formData = new FormDataXmlSerialization().LoadFromXml(out _, this.GetInitializationFilePath());
        if (formData is null)
            return;
        OnLoadFormData?.Invoke(formData);
        Size = formData.Size;
        Location = formData.Location;
        WindowState = formData.WindowState;
        DrawClient();
    }

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        var formData = OnSaveFormData?.Invoke();
        formData ??= new();
        formData.Size = Size;
        formData.Location = Location;
        formData.WindowState = WindowState;
        new FormDataXmlSerialization() { Source = formData }.SaveToXml(this.GetInitializationFilePath());
    }

    protected abstract void InitializeComponent();

    protected abstract void DrawClient();
}
