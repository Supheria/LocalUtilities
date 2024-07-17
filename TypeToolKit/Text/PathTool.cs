using LocalUtilities.TypeGeneral;
using System.Text;

namespace LocalUtilities.TypeToolKit.Text;

public static class PathTool
{
    public static string RenamePathByDateTime(this string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (dir is null)
            return path;
        var name = Path.GetFileNameWithoutExtension(path);
        name = new StringBuilder()
            .Append(name)
            .Append(SignCollection.UnderLine)
            .Append(DateTime.Now.ToString(DateTimeFormat.Data))
            .Append(SignCollection.Dot)
            .Append(Path.GetExtension(path))
            .ToString();
        return Path.Combine(dir, name);
    }
}
