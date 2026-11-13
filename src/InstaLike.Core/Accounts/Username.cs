using CSharpFunctionalExtensions;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Represents the non-empty name, of up to 20 characters, that an account logs in with.
/// </summary>
/// <remarks>Leading and trailing whitespace is removed when a username is created.</remarks>
public sealed class Username : SimpleValueObject<string>
{
    private const int UsernameMaxLength = 20;

    private Username() : base(string.Empty) { }

    private Username(string username) : base(username) { }

    public static Result<Username> Create(string username)
    {
        var input = (username ?? string.Empty).Trim();

        if (input.Length == 0)
        {
            return Result.Failure<Username>("Username should not be empty");
        }

        if (input.Length > UsernameMaxLength)
        {
            return Result.Failure<Username>($"Username is too long (max {UsernameMaxLength} chars)");
        }

        return Result.Success(new Username(input));
    }

    public static implicit operator string(Username username)
    {
        return username.Value;
    }

    public static explicit operator Username(string username)
    {
        return Create(username).Value;
    }
}
