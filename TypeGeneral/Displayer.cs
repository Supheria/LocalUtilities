using LocalUtilities.SimpleScript.Serialization;
using System;

namespace LocalUtilities.TypeGeneral;

public abstract class Displayer : PictureBox, ISsSerializable
{
    public string LocalName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Relocate()
    {
        if (Size == Image?.Size || Size.Width is 0 || Size.Height is 0)
            return;
        Image?.Dispose();
        Image = new Bitmap(Width, Height);
    }

    public void Deserialize(SsDeserializer deserializer)
    {
        throw new NotImplementedException();
    }

    public void Serialize(SsSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
