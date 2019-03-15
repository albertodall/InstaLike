using CSharpFunctionalExtensions;
using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class UnfollowCommand : IRequest<Result>
    {
        public int FollowerID { get; }
        public string UnfollowedNickname { get; }

        public UnfollowCommand(int followerID, string unfollowedNickname)
        {
            FollowerID = followerID;
            UnfollowedNickname = unfollowedNickname;
        }
    }
}
