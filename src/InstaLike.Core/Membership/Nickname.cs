using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership
{
    /// <summary>
    /// Represents a non-empty nickname of up to 20 characters.
    /// </summary>
    /// <remarks>Leading and trailing whitespace is removed when a nickname is created.</remarks>
    public sealed class Nickname : ValueObject<string>
    {
        internal const int NickNameMaxLength = 20;

        private Nickname() : base(string.Empty) { }

        private Nickname(string nickname) : base(nickname) { }

        public static Result<Nickname> Create(string nickname)
        {
            var input = (nickname ?? string.Empty).Trim();

            if (input.Length == 0)
            {
                return Result.Failure<Nickname>(MemberErrors.NicknameEmpty);
            }

            if (input.Length > NickNameMaxLength)
            {
                return Result.Failure<Nickname>(MemberErrors.NicknameTooLong);
            }

            return Result.Ok(new Nickname(input));
        }

        public static explicit operator Nickname(string nickname)
        {
            return Create(nickname).Value;
        }
    }
}
