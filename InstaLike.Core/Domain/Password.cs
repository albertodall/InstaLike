using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Password : ValueObject
    {
        private const byte MinimumPasswordLength = 6;
        private const byte SaltSize = 16;
        private const byte HashSize = 20;

        public virtual string HashedValue { get; }

        protected Password()
        { }

        private Password(string base64string)
        {
            HashedValue = base64string;
        }

        private Password(byte[] hash)
        {
            HashedValue = Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Creates a password from a plain text string.
        /// </summary>
        /// <param name="password">The password.</param>
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
            byte[] storedHashedPassword = Convert.FromBase64String(HashedValue);
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
            return password.HashedValue;
        }

        public static explicit operator Password(string base64password)
        {
            if (!IsBase64String(base64password))
            {
                throw new InvalidCastException("The specified password is not a valid base64 string");
            }
            return new Password(base64password);
        }

        public static explicit operator Password(byte[] hashBytes)
        {
            return new Password(hashBytes);
        }

        public override string ToString()
        {
            return HashedValue;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashedValue;
        }

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var csp = new RNGCryptoServiceProvider())
            {
                csp.GetBytes(salt);
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

        private static bool IsBase64String(string s)
        {
            return !string.IsNullOrEmpty(s)
                && s.Length != 0
                && s.Length % 4 == 0
                && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,2}$");
        }
    }
}