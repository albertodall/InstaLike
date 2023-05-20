using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class EditUserDetailsCommand : IRequest<Result>
    {
        public int UserID { get; }
        public string Nickname { get; }
        public string Name { get; }
        public string Surname { get; }
        public string Email { get; }
        public string Bio { get; }
        public byte[] ProfilePicture { get; }

        public EditUserDetailsCommand(int userID, string nickname, string name, string surname, string email, string bio, byte[] profilePicture)
        {
            UserID = userID;
            Nickname = nickname;
            Name = name;
            Surname = surname;
            Email = email;
            Bio = bio;
            ProfilePicture = profilePicture;
        }
    }
}
