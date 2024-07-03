using LocalUtilities.TypeToolKit.Text;

namespace LocalUtilities.IocpNet.Common;

public class UserInfo(string name, string password)
{
    public string Name { get; set; } = name;

    public string Password { get; set; } = password.ToMd5HashString();

    public UserInfo() : this("", "")
    {

    }

    public static bool operator ==(UserInfo? userInfo, object? obj)
    {
        if (userInfo is null)
        {
            if (obj is null)
                return true;
            else
                return false;
        }
        if (obj is not UserInfo other)
            return false;
        return userInfo.Name == other.Name && userInfo.Password == other.Password;
    }

    public static bool operator !=(UserInfo? userInfo, object? obj)
    {
        return !(userInfo == obj);
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
