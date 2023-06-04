using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Email : SimpleValueObject<string>
    {
        private const string EMailValidationRegEx = @"^(.+)@(.+)$";

        private Email() : base(string.Empty) { }

        private Email(string email) : base(email) { }

        public static Result<Email> Create(string input)
        {
            input = (input ?? string.Empty).Trim();

            if (input.Length == 0)
            {
                return Result.Failure<Email>("Email should not be empty");
            }

            if (!Regex.IsMatch(input, EMailValidationRegEx))
            {
                return Result.Failure<Email>("Email is invalid");
            }

            return Result.Success(new Email(input));
        }

        public static explicit operator Email(string email)
        {
            return Create(email).Value;
        }

        public static implicit operator string(Email email)
        {
            return email.Value;
        }
    }
}