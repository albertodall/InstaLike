using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Social.Commands;

public sealed class FollowCommand : ICommand<Result>
{
    public int FollowerID { get; }
    public string FollowedNickname { get; }

    public FollowCommand(int followerID, string followedNickname)
    {
        FollowerID = followerID;
        FollowedNickname = followedNickname;
    }
}
