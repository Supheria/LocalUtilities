using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript;
using LocalUtilities.SimpleScript.Common;

namespace LocalUtilities.TypeGeneral;

public abstract class ResizeableForm : Form, IInitializeable
{

    protected delegate FormData SerializeHandler();

    protected delegate void DeserializeHandler(object? data);

    protected delegate void DrawClientHandler();

    protected event SerializeHandler? OnSaveForm;

    protected event DeserializeHandler? OnLoadForm;

    protected event DrawClientHandler? OnDrawClient;

    public abstract string InitializeName { get; }

    public string IniFileExtension { get; } = "form";

    bool Resizing { get; set; } = false;

    protected new int Padding { get; set; } = 12;

    protected virtual FontData LabelFontData { get; set; } = new();

    protected virtual FontData ContentFontData { get; set; } = new() { Size = 17f };

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

    protected virtual Type DataType => typeof(FormData);

    protected static SsSignTable SignTable { get; } = new();

    protected class FormData
    {
        public Size MinimumSize { get; set; }

        public Size Size { get; set; }

        public Point Location { get; set; }

        public FormWindowState WindowState { get; set; }

        public int Padding { get; set; }

        public FontData LabelFontData { get; set; } = new();

        public FontData ContentFontData { get; set; } = new();
    }

    public ResizeableForm()
    {
        ResizeBegin += ResizeableForm_ResizeBegin;
        ResizeEnd += ResizeableForm_ResizeEnd;
        SizeChanged += ResizeableForm_SizeChanged;
        Load += ResizeableForm_Load;
        //Shown += ResizeableForm_Shown;
        FormClosing += ResizeableForm_FormClosing;
        //InitializeComponent();
    }

    private void ResizeableForm_Shown(object? sender, EventArgs e)
    {
        DrawClient();
    }

    //protected abstract void InitializeComponent();

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

    private void DrawClient()
    {
        if (WindowState is FormWindowState.Minimized)
            return;
        SuspendLayout();
        OnDrawClient?.Invoke();
        ResumeLayout();
    }

    private void ResizeableForm_Load(object? sender, EventArgs e)
    {
        try
        {
            var data = SerializeTool.DeserializeFile(DataType, new(InitializeName), this.GetInitializeFilePath(), SignTable);
            OnLoadForm?.Invoke(data);
            if (data is not FormData formData)
                return;
            MinimumSize = formData.MinimumSize;
            Size = formData.Size;
            Location = formData.Location;
            WindowState = formData.WindowState;
            Padding = formData.Padding;
            LabelFontData = formData.LabelFontData;
            ContentFontData = formData.ContentFontData;

        }
        catch { }
        DrawClient();
    }

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        try
        {
            var formData = OnSaveForm?.Invoke() ?? new();
            formData.MinimumSize = MinimumSize;
            formData.Size = Size;
            formData.Location = Location;
            formData.WindowState = WindowState;
            formData.Padding = Padding;
            formData.LabelFontData = LabelFontData;
            formData.ContentFontData = ContentFontData;
            SerializeTool.SerializeFile(formData, new(InitializeName), this.GetInitializeFilePath(), true, SignTable);
        }
        catch { }
    }

    protected void InvokeAsync(Action process)
    {
        if (InvokeRequired)
            BeginInvoke(process);
        else
            Invoke(process);
    }
}
