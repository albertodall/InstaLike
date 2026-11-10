using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Social;

public sealed class FollowCommand : IRequest<Result>
{
    public int FollowerID { get; }
    public string FollowedNickname { get; }

    public FollowCommand(int followerID, string followedNickname)
    {
        FollowerID = followerID;
        FollowedNickname = followedNickname;
    }
}
