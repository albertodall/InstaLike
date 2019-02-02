using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Password : ValueObject
    {
        private const byte MinimumPasswordLength = 6;

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

            return Result.Ok(new Password(password));

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
    }
}