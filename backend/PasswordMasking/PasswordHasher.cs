using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace EMSSolution.PasswordMasking
{
    public class PasswordHasher
    {
        public static (string hashedPassword, string salt) HashPassword(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16); // 128-bit salt

            string salt = Convert.ToBase64String(saltBytes);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return (hashed, salt);
        }

        public static bool VerifyPassword(string password, string? storedHash, string? storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            string hashToCheck = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashToCheck == storedHash;
        }
    }
}
