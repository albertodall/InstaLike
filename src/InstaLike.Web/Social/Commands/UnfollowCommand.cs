using System;
using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Social.Commands
{
    public sealed class UnfollowCommand : ICommand<Result>
    {
        public Guid FollowerID { get; }
        public string UnfollowedNickname { get; }

        public UnfollowCommand(Guid followerID, string unfollowedNickname)
        {
            FollowerID = followerID;
            UnfollowedNickname = unfollowedNickname;
        }
    }
}
