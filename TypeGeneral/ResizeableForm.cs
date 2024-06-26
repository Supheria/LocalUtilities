﻿using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.TypeGeneral.Convert;

namespace LocalUtilities.TypeGeneral;

public delegate void FormOnSaveing(SsSerializer serializer);

public delegate void FormOnLoading(SsDeserializer deserializer);

public abstract class ResizeableForm : Form, ISsSerializable
{
    protected event FormOnSaveing? OnSaveForm;

    protected event FormOnLoading? OnLoadForm;

    protected event OnComponentRunning? OnDrawClient;

    public abstract string LocalName { get; }

    public string? IniFilePath { get; set; }

    bool Resizing { get; set; } = false;

    protected new int Padding { get; set; } = 12;

    protected virtual FontData LabelFontData { get; set; } = new(nameof(LabelFontData));

    protected virtual FontData ContentFontData { get; set; } = new(nameof(ContentFontData)) { Size = 17f };

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

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
        if (IniFilePath is null || IniFilePath is "")
            _ = this.LoadFromSimpleScript();
        else
        {
            try
            {
                _ = this.LoadFromSimpleScript(IniFilePath);
            }
            catch
            {
                _ = this.LoadFromSimpleScript();
            }
        }
        DrawClient();
    }

    private void ResizeableForm_FormClosing(object? sender, FormClosingEventArgs e)
    {

        if (IniFilePath is null || IniFilePath is "")
            this.SaveToSimpleScript(true);
        else
            this.SaveToSimpleScript(true, IniFilePath);
    }

    protected void InvokeAsync(Action process)
    {
        if (InvokeRequired)
            BeginInvoke(process);
        else
            Invoke(process);
    }

    public void Serialize(SsSerializer serializer)
    {
        serializer.WriteTag(nameof(MinimumSize), MinimumSize.ToArrayString());
        serializer.WriteTag(nameof(Size), Size.ToArrayString());
        serializer.WriteTag(nameof(Location), Location.ToArrayString());
        serializer.WriteTag(nameof(WindowState), WindowState.ToString());
        serializer.WriteTag(nameof(Padding), Padding.ToString());
        serializer.WriteObject(LabelFontData);
        serializer.WriteObject(ContentFontData);
        OnSaveForm?.Invoke(serializer);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        MinimumSize = deserializer.ReadTag(nameof(MinimumSize), s => s.ToSize());
        Size = deserializer.ReadTag(nameof(Size), s => s.ToSize());
        Location = deserializer.ReadTag(nameof(Location), s => s.ToPoint());
        WindowState = deserializer.ReadTag(nameof(WindowState), s => s.ToEnum<FormWindowState>());
        Padding = deserializer.ReadTag(nameof(Padding), int.Parse);
        deserializer.ReadObject(LabelFontData);
        deserializer.ReadObject(ContentFontData);
        OnLoadForm?.Invoke(deserializer);
    }
}
