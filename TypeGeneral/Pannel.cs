namespace LocalUtilities.TypeGeneral;

public class Pannel : Control
{
    protected Rectangle ClientRect => ClientRectangle;

    protected int ClientLeft => ClientRectangle.Left;

    protected int ClientTop => ClientRectangle.Top;

    protected int ClientWidth => ClientRectangle.Width;

    protected int ClientHeight => ClientRectangle.Height;

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

    }

    public virtual void DisableListener()
    {

    }
}
