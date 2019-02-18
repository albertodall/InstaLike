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

        private Password(string base64string)
        {
            Value = base64string;
        }

        private Password(byte[] hash)
        {
            Value = Convert.ToBase64String(hash);
        }

        public static Result<Password> Create(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Result.Fail<Password>("Password cannot be empty.");
            }

            if (password.Length < MinimumPasswordLength)
            {
                return Result.Fail<Password>($"Password is too short. Minimum allowed length is {MinimumPasswordLength} characters. ");
            }

            return Result.Ok(new Password(HashPassword(password)));

        }

        public bool HashMatches(string password)
        {
            byte[] hashedPassword;
            byte[] storedHashedPassword = Convert.FromBase64String(Value);
            byte[] saltPart = new byte[SaltSize];
            Array.Copy(storedHashedPassword, 0, saltPart, 0, SaltSize);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltPart, 10000))
            {
                hashedPassword = saltPart.Concat(pbkdf2.GetBytes(HashSize)).ToArray();
            }
            return hashedPassword.SequenceEqual(storedHashedPassword);
        }

        public static implicit operator string(Password password)
        {
            return password.Value;
        }

        public static explicit operator Password(string password)
        {
            return Create(password).Value;
        }

        public static explicit operator Password(byte[] hashBytes)
        {
            return new Password(hashBytes);
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
            string hashedPassword = string.Empty;

            byte[] salt;
            using (var csp = new RNGCryptoServiceProvider())
            {
                csp.GetBytes(salt = new byte[SaltSize]);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            return Convert.ToBase64String(hashBytes);
        }
    }
}