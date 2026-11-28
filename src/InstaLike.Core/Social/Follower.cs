using System.Collections.Generic;
using System.Linq;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social
{
    /// <summary>
    /// Represents a member as someone who follows other members.
    /// </summary>
    /// <remarks>
    /// A member becomes a follower with their first follow; members who never followed anyone have no follower.
    /// </remarks>
    public class Follower : AggregateRoot<MemberId>
    {
        protected Follower() { }

        public Follower(MemberId memberID)
            : base(memberID)
        {
        }

        private readonly IList<Follow> _follows = [];
        public virtual IReadOnlyCollection<Follow> Follows => _follows.AsReadOnly();

        public virtual Result Follow(MemberId followedID)
        {
            if (followedID == ID)
            {
                return Result.Failure(FollowerErrors.SelfFollow);
            }

            if (IsFollowing(followedID))
            {
                return Result.Failure(FollowerErrors.AlreadyFollowing);
            }

            _follows.Add(new Follow(this, followedID));
            RaiseDomainEvent(new MemberFollowedEvent(ID, followedID));
            return Result.Ok();
        }

        public virtual Result Unfollow(MemberId followedID)
        {
            var follow = _follows.SingleOrDefault(f => f.FollowedID == followedID);
            if (follow is null)
            {
                return Result.Failure(FollowerErrors.NotFollowing);
            }

            _follows.Remove(follow);
            return Result.Ok();
        }

        public virtual bool IsFollowing(MemberId followedID)
        {
            return _follows.Any(f => f.FollowedID == followedID);
        }
    }
}
