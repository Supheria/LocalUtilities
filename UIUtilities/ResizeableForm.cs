using LocalUtilities.Interface;

namespace LocalUtilities.UIUtilities;

public abstract class ResizeableForm : Form, IInitializationManageable
{
    string _iniFileName { get; }

    public string IniFileName => _iniFileName;

    bool _resizing { get; set; } = false;


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

    private void ResizeableForm_Load(object? sender, EventArgs e) => LoadInitializationData();

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e) => SaveInitializationData();

    protected abstract void DrawClient();

    protected abstract void LoadInitializationData();

    protected abstract void SaveInitializationData();

    protected abstract void InitializeComponent();
}
