using LocalUtilities.FileUtilities;
using LocalUtilities.Interface;

namespace LocalUtilities.UIUtilities;

public abstract class ResizeableForm : Form, IInitializationManageable
{
    string _iniFileName { get; }

    public string IniFileName => _iniFileName;

    bool _resizing { get; set; } = false;

    protected FormDataLoadDelegate? OnLoadFormData;

    protected FormDataSaveDelegate? OnSaveFormData;

    protected FormDataXmlSerialization FormDataXmlSerialization { get; set; } = new();


    public ResizeableForm(string iniFileName)
    {
        _iniFileName = iniFileName;
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
        var formData = FormDataXmlSerialization.LoadFromXml(this.GetInitializationFilePath());
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
        formData.SaveToXml(this.GetInitializationFilePath(), FormDataXmlSerialization);
    }

    protected abstract void InitializeComponent();

    protected abstract void DrawClient();
}
