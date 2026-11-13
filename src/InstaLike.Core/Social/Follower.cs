using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social;

/// <summary>
/// Represents a member as someone who follows other members.
/// </summary>
/// <remarks>
/// A member becomes a follower with their first follow; members who never followed anyone have no follower.
/// </remarks>
public class Follower : AggregateRoot<FollowerId>
{
    protected Follower() { }

    public Follower(Guid memberID)
        : base(FollowerId.From(memberID))
    {
    }

    private readonly IList<Follow> _follows = [];
    public virtual IReadOnlyCollection<Follow> Follows => _follows.AsReadOnly();

    public virtual Result Follow(Guid followedID)
    {
        if (followedID == ID.Value)
        {
            return Result.Failure("You cannot follow yourself.");
        }

        if (IsFollowing(followedID))
        {
            return Result.Failure("You are already following this member.");
        }

        _follows.Add(new Follow(this, followedID));
        RaiseDomainEvent(new MemberFollowedEvent(ID.Value, followedID));
        return Result.Success();
    }

    public virtual Result Unfollow(Guid followedID)
    {
        var follow = _follows.SingleOrDefault(f => f.FollowedID == followedID);
        if (follow is null)
        {
            return Result.Failure("You are not following this member.");
        }

        _follows.Remove(follow);
        return Result.Success();
    }

    public virtual bool IsFollowing(Guid followedID)
    {
        return _follows.Any(f => f.FollowedID == followedID);
    }
}
