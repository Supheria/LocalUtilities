using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript;
using LocalUtilities.SimpleScript.Common;
using System.ComponentModel;

namespace LocalUtilities.TypeGeneral;

public abstract class ResizeableForm : Form, IInitializeable
{
    public abstract string InitializeName { get; }

    public string IniFileExtension { get; } = "form";

    bool Resizing { get; set; } = false;

    protected new int Padding { get; set; } = 12;

    protected virtual FontData LabelFontData { get; set; } = new();

    protected virtual FontData ContentFontData { get; set; } = new() { Size = 17f };

    protected Rectangle ClientRect => ClientRectangle;

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

    protected virtual Type FormDataType => typeof(FormData);

    protected static SsSignTable SignTable { get; } = new();

    protected class FormData
    {
        public virtual Size MinimumSize { get; set; }

        public virtual Size Size { get; set; }

        public virtual Point Location { get; set; }

        public virtual FormWindowState WindowState { get; set; }

        public virtual int Padding { get; set; }

        public virtual FontData LabelFontData { get; set; } = new();

        public virtual FontData ContentFontData { get; set; } = new();
    }

    public ResizeableForm()
    {
        AddOperation();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        DrawClient();
    }

    protected override void OnResizeBegin(EventArgs e)
    {
        base.OnResizeBegin(e);
        Resizing = true;
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);
        Resizing = false;
        DrawClient();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Resizing is false)
            DrawClient();
    }

    protected virtual void AddOperation()
    {

    }

    private void DrawClient()
    {
        if (WindowState is FormWindowState.Minimized)
            return;
        Redraw();
    }

    protected virtual void Redraw()
    {

    }

    protected sealed override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        try
        {
            var data = SerializeTool.DeserializeFile(FormDataType, new(InitializeName), SignTable, this.GetInitializeFilePath());
            OnLoad(data);
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
    }

    protected virtual void OnLoad(object? formData)
    {

    }

    protected sealed override void OnClosing(CancelEventArgs e)
    {
        try
        {
            var formData = OnSave();
            formData.MinimumSize = MinimumSize;
            formData.Size = Size;
            formData.Location = Location;
            formData.WindowState = WindowState;
            formData.Padding = Padding;
            formData.LabelFontData = LabelFontData;
            formData.ContentFontData = ContentFontData;
            SerializeTool.SerializeFile(formData, new(InitializeName), SignTable, true, this.GetInitializeFilePath());
        }
        catch { }
    }

    protected virtual FormData OnSave()
    {
        return new();
    }
}
