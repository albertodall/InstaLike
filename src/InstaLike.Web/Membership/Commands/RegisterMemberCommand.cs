using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Data;

namespace InstaLike.Web.Membership.Commands
{
    public sealed class RegisterMemberCommand : ICommand<Result<MemberId>>
    {
        public string Nickname { get; }
        public string Name { get; }
        public string Surname { get; }
        public string Password { get; }
        public string Email { get; }
        public string Biography { get; }
        public byte[]? ProfilePicture { get; }

        public RegisterMemberCommand(string nickname, string name, string surname, string password, string email, string biography, byte[]? profilePicture)
        {
            Nickname = nickname;
            Name = name;
            Surname = surname;
            Password = password;
            Email = email;
            Biography = biography;
            ProfilePicture = profilePicture;
        }
    }
}
