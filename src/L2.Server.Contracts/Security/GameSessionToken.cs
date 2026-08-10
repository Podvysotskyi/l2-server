using System.Security.Cryptography;
using System.Text;

namespace L2.Server.Contracts.Security;

public static class GameSessionToken
{
    public static string Create() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    public static byte[] Hash(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
