namespace LocalUtilities.TypeGeneral;

public class Pannel : Control
{
    protected Rectangle ClientRect => ClientRectangle;

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

    protected int ClientRight => ClientRectangle.Right;

    public virtual new Size Padding { get; set; }

    public Pannel()
    {
        AddOperation();
    }

    protected virtual void AddOperation()
    {

    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        SetSize();
        //BeginInvoke(() =>
        //{
        //    //SuspendLayout();
        //    SetSize();
        //    //ResumeLayout();
        //});
    }

    protected virtual void SetSize()
    {

    }

    public virtual void EnableListener()
    {
        foreach (var control in Controls)
        {
            if (control is Displayer displayer)
                displayer.EnableListener();
            else if (control is Pannel pannel)
                pannel.EnableListener();
        }
    }

    public virtual void DisableListener()
    {
        foreach (var control in Controls)
        {
            if (control is Displayer displayer)
                displayer.DisableListener();
            else if (control is Pannel pannel)
                pannel.DisableListener();
        }
    }
}
