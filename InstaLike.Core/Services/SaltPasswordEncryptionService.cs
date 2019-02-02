using System;
using System.Security.Cryptography;

namespace InstaLike.Core.Services
{
    public sealed class SaltPasswordEncryptionService : IPasswordEncryptionService
    {
        private const byte SaltSize = 16;
        private const byte HashSize = 20;

        public string Encrypt(string password)
        {
            string encryptedPassword = string.Empty;

            byte[] salt;
            using (var csp = new RNGCryptoServiceProvider())
            {
                csp.GetBytes(salt = new byte[SaltSize]);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
                encryptedPassword = Convert.ToBase64String(hashBytes);
            }

            return encryptedPassword;
        }
    }
}
