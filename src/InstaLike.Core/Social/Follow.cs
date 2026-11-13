using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social
{
    /// <summary>
    /// Represents a follower following another member.
    /// </summary>
    public class Follow : EntityBase<Guid>
    {
#pragma warning disable CS8618
        protected Follow() { }
#pragma warning restore CS8618

        internal Follow(Follower follower, Guid followedID)
            : this()
        {
            Follower = follower;
            FollowedID = followedID;
            FollowDate = DateTimeOffset.Now;
        }

        public virtual Follower Follower { get; protected set; }

        public virtual Guid FollowedID { get; protected set; }

        public virtual DateTimeOffset FollowDate { get; protected set; }
    }
}
