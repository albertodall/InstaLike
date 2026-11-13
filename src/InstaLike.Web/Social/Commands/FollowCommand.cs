using System;
using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Social.Commands;

public sealed class FollowCommand : ICommand<Result>
{
    public Guid FollowerID { get; }
    public string FollowedNickname { get; }

    public FollowCommand(Guid followerID, string followedNickname)
    {
        FollowerID = followerID;
        FollowedNickname = followedNickname;
    }
}
