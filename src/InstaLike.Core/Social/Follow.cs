using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

public class Follow : EntityBase<int>
{
    protected Follow() { }

    public Follow(int followerID, int followedID)
        : this()
    {
        FollowerID = followerID;
        FollowedID = followedID;
        FollowDate = DateTimeOffset.Now;
    }

    public virtual int FollowerID { get; protected set; }

    public virtual int FollowedID { get; protected set; }

    public virtual DateTimeOffset FollowDate { get; protected set; }
}
