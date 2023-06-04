using CSharpFunctionalExtensions;

namespace InstaLike.Core.Domain
{
    public class Nickname : SimpleValueObject<string>
    {
        private const int NickNameMaxLength = 20;

        private Nickname() : base(string.Empty) { }

        private Nickname(string nickname) : base(nickname) { }

        public static Result<Nickname> Create(string nickname)
        {
            var input = (nickname ?? string.Empty).Trim();

            if (input.Length == 0)
            {
                return Result.Failure<Nickname>("Nickname should not be empty");
            }

            if (input.Length > NickNameMaxLength)
            {
                return Result.Failure<Nickname>($"Nickname is too long (max {NickNameMaxLength} chars)");
            }

            return Result.Success(new Nickname(input));
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