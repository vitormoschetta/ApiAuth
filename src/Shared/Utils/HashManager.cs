using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Shared.Utils
{
    public class HashManager
    {       
        public static string GenerateHash(string input, byte[] salt)
        {
            var hashed = KeyDerivation.Pbkdf2(
                               password: input,
                               salt: salt,
                               prf: KeyDerivationPrf.HMACSHA256,
                               iterationCount: 100000,
                               numBytesRequested: 256 / 8);

            return Convert.ToBase64String(hashed);
        }

        public static byte[] GenerateSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password, byte[] salt)
        {
            var hashedInput = GenerateHash(password, salt);
            return hashedPassword == hashedInput;
        }
    }
}