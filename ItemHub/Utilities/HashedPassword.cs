using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ItemHub.Utilities;

public static class HashedPassword
{
    public static byte[] GeneratedSalt => RandomNumberGenerator.GetBytes(16);
    public static string Hashed(string password, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 87654,
            numBytesRequested: 32));
    }
}