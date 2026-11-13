using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

public class Follow : AggregateRoot<FollowId>
{
    protected Follow() { }

    private Follow(Guid followerID, Guid followedID)
        : base(FollowId.New())
    {
        FollowerID = followerID;
        FollowedID = followedID;
        FollowDate = DateTimeOffset.Now;
    }

    public virtual Guid FollowerID { get; protected set; }

    public virtual Guid FollowedID { get; protected set; }

    public virtual DateTimeOffset FollowDate { get; protected set; }

    public static Result<Follow> Create(Guid followerID, Guid followedID)
    {
        if (followerID == followedID)
        {
            return Result.Failure<Follow>("You cannot follow yourself.");
        }

        var follow = new Follow(followerID, followedID);
        follow.RaiseDomainEvent(new FollowedUserEvent(followerID, followedID));
        return Result.Success(follow);
    }
}
