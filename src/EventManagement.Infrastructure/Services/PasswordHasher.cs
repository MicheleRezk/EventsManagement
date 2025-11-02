using System.Security.Cryptography;
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace EventManagement.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            100000,
            32));

        return $"{Convert.ToBase64String(salt)}:{hashed}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var hash = parts[1];

        var testHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            100000,
            32));

        return hash == testHash;
    }
}