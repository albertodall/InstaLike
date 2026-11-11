using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

public class Follow : EntityBase<int>
{
    protected Follow() { }

    private Follow(int followerID, int followedID)
        : this()
    {
        FollowerID = followerID;
        FollowedID = followedID;
        FollowDate = DateTimeOffset.Now;
    }

    public virtual int FollowerID { get; protected set; }

    public virtual int FollowedID { get; protected set; }

    public virtual DateTimeOffset FollowDate { get; protected set; }

    public static Result<Follow> Create(int followerID, int followedID)
    {
        return followerID == followedID
            ? Result.Failure<Follow>("You cannot follow yourself.")
            : Result.Success(new Follow(followerID, followedID));
    }
}
