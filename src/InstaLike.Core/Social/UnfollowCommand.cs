using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

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
