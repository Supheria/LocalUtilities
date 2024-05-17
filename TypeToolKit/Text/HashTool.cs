using System.Security.Cryptography;
using System.Text;

namespace LocalUtilities.TypeToolKit.Text;

public static class HashTool
{
    public static string ToMd5HashString(this string str)
    {
        if (str is null)
            return "";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));
        return hash.Aggregate(string.Empty, (current, b) => current + b);
    }

    public static string ToMd5HashString(this FileStream file)
    {
        var sha = MD5.Create();
        var hash = sha.ComputeHash(file);
        return hash.Aggregate(string.Empty, (current, b) => current + b);
    }
}