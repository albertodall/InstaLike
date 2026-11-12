using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Represents a password stored as a Base64-encoded salted hash and supports verification of plain-text passwords.
/// </summary>
/// <remarks>
/// Passwords created through this type must contain at least six characters.
/// </remarks>
public sealed class Password : ValueObject
{
    private const byte MinimumPasswordLength = 6;
    private const byte SaltSize = 16;
    private const byte HashSize = 20;
    private const int HashIterations = 10000;

    private string HashedValue { get; }

#pragma warning disable CS8618
    private Password() { }
#pragma warning restore CS8618

    private Password(string base64String) : this()
    {
        HashedValue = base64String;
    }

    private Password(byte[] hash) : this()
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
            return Result.Failure<Password>("Password cannot be empty.");
        }

        if (password.Length < MinimumPasswordLength)
        {
            return Result.Failure<Password>($"Password is too short. Minimum allowed length is {MinimumPasswordLength} characters. ");
        }

        return Result.Success(new Password(HashPassword(password)));
    }

    public bool HashMatches(string password)
    {
        var storedHashedPassword = Convert.FromBase64String(HashedValue);
        var saltPart = storedHashedPassword.AsSpan(0, SaltSize).ToArray();
        var hashedPassword = saltPart.Concat(DeriveHash(password, saltPart)).ToArray();
        return CryptographicOperations.FixedTimeEquals(hashedPassword, storedHashedPassword);
    }

    public static implicit operator string(Password password)
    {
        return password.HashedValue;
    }

    public static explicit operator Password(string base64Password)
    {
        if (!IsBase64String(base64Password))
        {
            throw new InvalidCastException("The specified password is not a valid base64 string");
        }

        return new Password(base64Password);
    }

    public static explicit operator Password(byte[] hashBytes)
    {
        return new Password(hashBytes);
    }

    public override string ToString()
    {
        return HashedValue;
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return HashedValue;
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = DeriveHash(password, salt);
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
        return Convert.ToBase64String(hashBytes);
    }

    /// <remarks>
    /// SHA1 and the iteration count must not change: they are needed to verify the already stored passwords.
    /// </remarks>
    private static byte[] DeriveHash(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA1, HashSize);
    }

    private static bool IsBase64String(string s)
    {
        return !string.IsNullOrEmpty(s)
            && s.Length != 0
            && s.Length % 4 == 0
            && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,2}$");
    }
}
