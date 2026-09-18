using System.Security.Cryptography;
using System.Text;
using Users.Application.Contracts.Services;

namespace Users.Application.Services.Auth;

public class HashService : IHashService
{
    public string Hash(string password)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string hash)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        if (hash is null) throw new ArgumentNullException(nameof(hash));
        var computed = Hash(password);
        return string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase);
    }
}
