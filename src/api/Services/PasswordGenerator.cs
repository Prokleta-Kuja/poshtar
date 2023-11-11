using System.Security.Cryptography;
using System.Text;

namespace poshtar.Services;

public static class SecretGenerator
{
    const string CHARS = "./-?:.,_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    static readonly byte[] s_chars = CHARS.Select(c => Convert.ToByte(c)).ToArray();
    public static string Password(int length)
    {
        var passwordArray = new byte[length];
        for (int i = 0; i < length; i++)
            passwordArray[i] = s_chars[RandomNumberGenerator.GetInt32(length)];

        var password = Encoding.UTF8.GetString(passwordArray);
        return password;
    }
}