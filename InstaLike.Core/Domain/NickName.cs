using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Nickname : ValueObject
    {
        public string Value { get; }

        protected Nickname()
        { }

        private Nickname(string nickname)
        {
            Value = nickname;
        }

        public static Result<Nickname> Create(string nickname)
        {
            var nick = (nickname ?? string.Empty).Trim();

            if (nick.Length == 0)
            {
                return Result.Fail<Nickname>("Nickname should not be empty");
            }

            if (nick.Length > 20)
            {
                return Result.Fail<Nickname>("Nickname is too long (max 20 chars)");
            }

            return Result.Ok(new Nickname(nick));
        }

        public static implicit operator string(Nickname nickname)
        {
            return nickname.Value;
        }

        public static explicit operator Nickname(string nickname)
        {
            return Create(nickname).Value;
        }

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToUpperInvariant();
        }
    }
}