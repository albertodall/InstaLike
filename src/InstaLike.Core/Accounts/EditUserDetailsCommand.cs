using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

public sealed class EditUserDetailsCommand : ICommand<Result>
{
    public int UserID { get; }
    public string Nickname { get; }
    public string Name { get; }
    public string Surname { get; }
    public string Email { get; }
    public string Bio { get; }
    /// <summary>The new profile picture, or <see langword="null"/> to keep the current one.</summary>
    public byte[]? ProfilePicture { get; }

    public EditUserDetailsCommand(int userID, string nickname, string name, string surname, string email, string bio, byte[]? profilePicture)
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
