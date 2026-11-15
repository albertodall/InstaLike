using System.Text.RegularExpressions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Represents a normalized email address.
    /// </summary>
    /// <remarks>
    /// Email values are trimmed, converted to lowercase, and checked for a basic address format
    /// when created.
    /// </remarks>
    public sealed class Email : ValueObject<string>
    {
        private const string EMailValidationRegEx = @"^(.+)@(.+)$";

        private Email() : base(string.Empty) { }

        private Email(string email) : base(email) { }

        public static Result<Email> Create(string input)
        {
            input = (input ?? string.Empty).Trim().ToLowerInvariant();

            if (input.Length == 0)
            {
                return Result.Failure<Email>(AccountErrors.EmailEmpty);
            }

            if (!Regex.IsMatch(input, EMailValidationRegEx))
            {
                return Result.Failure<Email>(AccountErrors.EmailInvalid);
            }

            return Result.Ok(new Email(input));
        }

        public static explicit operator Email(string email)
        {
            return Create(email).Value;
        }
    }
}
