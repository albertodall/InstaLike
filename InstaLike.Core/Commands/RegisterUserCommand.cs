namespace InstaLike.Core.Commands
{
    public class RegisterUserCommand : ICommand
    {
        public string Nickname { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
        public string Email { get; }
        public string Biography { get; }
        public byte[] ProfilePicture { get; }

        public RegisterUserCommand(string nickname, string firstName, string lastName, string password, string email, string biography, byte[] profilePicture)
        {
            Nickname = nickname;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            Email = email;
            Biography = biography;
            ProfilePicture = profilePicture;
        }
    }
}
