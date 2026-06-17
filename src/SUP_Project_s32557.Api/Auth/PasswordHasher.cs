using System.Security.Cryptography;
using System.Text;

namespace SUP_Project_s32557.Api.Auth;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
