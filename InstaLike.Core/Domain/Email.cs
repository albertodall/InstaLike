using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Email : ValueObject
    {
        public string Value { get; }

        private Email() { }

        private Email(string email) : this()
        {
            Value = email;
        }

        public static Result<Email> Create(string email)
        {
            email = (email ?? string.Empty).Trim();

            if (email.Length == 0)
            {
                return Result.Failure<Email>("Email should not be empty");
            }

            if (!Regex.IsMatch(email, @"^(.+)@(.+)$"))
            {
                return Result.Failure<Email>("Email is invalid");
            }

            return Result.Success(new Email(email));
        }

        public override string ToString()
        {
            return Value;
        }

        public static explicit operator Email(string email)
        {
            return Create(email).Value;
        }

        public static implicit operator string(Email email)
        {
            return email.Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }
    }
}