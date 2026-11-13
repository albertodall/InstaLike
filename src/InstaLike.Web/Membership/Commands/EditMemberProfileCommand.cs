using CSharpFunctionalExtensions;
using InstaLike.Core.Membership;
using InstaLike.Web.Data;

namespace InstaLike.Web.Membership.Commands;

public sealed class EditMemberProfileCommand : ICommand<Result>
{
    public MemberId MemberID { get; }
    public string Nickname { get; }
    public string Name { get; }
    public string Surname { get; }
    public string Bio { get; }
    /// <summary>The new profile picture, or <see langword="null"/> to keep the current one.</summary>
    public byte[]? ProfilePicture { get; }

    public EditMemberProfileCommand(MemberId memberID, string nickname, string name, string surname, string bio, byte[]? profilePicture)
    {
        MemberID = memberID;
        Nickname = nickname;
        Name = name;
        Surname = surname;
        Bio = bio;
        ProfilePicture = profilePicture;
    }
}
