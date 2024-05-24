using LocalUtilities.SimpleScript.Serialization;

namespace LocalUtilities.TypeGeneral;

public delegate void DisplayerOnRunning();

public abstract class Displayer : PictureBox, ISsSerializable
{
    public string LocalName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected DisplayerOnRunning? OnRelocate;

    public void SetRange(Size range)
    {
        SuspendLayout();
        if (OnSetRange(range) && Size != Image?.Size)
        {
            Image?.Dispose();
            Image = new Bitmap(Width, Height);
            var g = Graphics.FromImage(Image);
            g.Flush();
            g.Dispose();
            Relocate();
        }
        ResumeLayout();
    }

    protected abstract bool OnSetRange(Size range);

    protected abstract void Relocate();

    public void Deserialize(SsDeserializer deserializer)
    {
        throw new NotImplementedException();
    }

    public void Serialize(SsSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
