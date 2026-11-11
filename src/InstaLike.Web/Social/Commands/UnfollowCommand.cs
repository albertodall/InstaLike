using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Social.Commands;

public sealed class UnfollowCommand : ICommand<Result>
{
    public int FollowerID { get; }
    public string UnfollowedNickname { get; }

    public UnfollowCommand(int followerID, string unfollowedNickname)
    {
        FollowerID = followerID;
        UnfollowedNickname = unfollowedNickname;
    }
}
