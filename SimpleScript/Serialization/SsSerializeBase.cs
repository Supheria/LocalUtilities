using LocalUtilities.FileHelper;
using LocalUtilities.SimpleScript.Common;

namespace LocalUtilities.SimpleScript.Serialization;

public abstract class SsSerializeBase(object obj) : IInitializeable
{
    public ISsSerializable Source { get; set; } = obj as ISsSerializable ?? throw new SsParseExceptions("type of obj must be ISsSerializable");

    public string IniFileName => Source.LocalName;

    public string IniFileExtension => "ss";
}
