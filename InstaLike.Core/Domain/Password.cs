using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Password : ValueObject
    {
        private const byte MinimumPasswordLength = 6;
        private const byte SaltSize = 16;
        private const byte HashSize = 20;

        public virtual string Value { get; }

        protected Password()
        { }

        private Password(string password)
        {
            Value = password;
        }

        public static Result<Password> Create(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Result.Fail<Password>("Password cannot be empty.");
            }

            if (password.Length < MinimumPasswordLength)
            {
                return Result.Fail<Password>("Password is too short. Minimum allowed length is 6 characters. ");
            }

            return Result.Ok(new Password(HashPassword(password)));

        }

        public static implicit operator string(Password password)
        {
            return password.Value;
        }

        public static explicit operator Password(string password)
        {
            return Create(password).Value;
        }

        public override string ToString()
        {
            return Enumerable.Repeat('*', MinimumPasswordLength).ToString();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        private static string HashPassword(string password)
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