using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Membership
{
    public static class MemberErrors
    {
        public static Error NotFound(string nickname) =>
            Error.NotFound("Member.NotFound", $"Member [{nickname}] does not exist.");

        public static readonly Error NicknameTaken =
            Error.Conflict("Nickname.Taken", "This nickname is already taken.");

        public static readonly Error NicknameEmpty =
            Error.Validation("Nickname.Empty", "Nickname should not be empty");

        public static readonly Error NicknameTooLong =
            Error.Validation("Nickname.TooLong", $"Nickname is too long (max {Nickname.NickNameMaxLength} chars)");

        public static readonly Error NameEmpty =
            Error.Validation("Name.Empty", "You have to specify a proper name. An empty string is not allowed.");

        public static readonly Error SurnameEmpty =
            Error.Validation("Surname.Empty", "You have to specify a proper surname. An empty string is not allowed.");
    }
}
