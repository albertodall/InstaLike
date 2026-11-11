using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

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
