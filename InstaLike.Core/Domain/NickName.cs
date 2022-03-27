using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Nickname : SimpleValueObject<string>
    {
        private Nickname() : base(string.Empty) { }

        private Nickname(string nickname) : base(nickname) { }

        public static Result<Nickname> Create(string nickname)
        {
            var nick = (nickname ?? string.Empty).Trim();

            if (nick.Length == 0)
            {
                return Result.Failure<Nickname>("Nickname should not be empty");
            }

            if (nick.Length > 20)
            {
                return Result.Failure<Nickname>("Nickname is too long (max 20 chars)");
            }

            return Result.Success(new Nickname(nick));
        }

        public static implicit operator string(Nickname nickname)
        {
            return nickname.Value;
        }

        public static explicit operator Nickname(string nickname)
        {
            return Create(nickname).Value;
        }
    }
}