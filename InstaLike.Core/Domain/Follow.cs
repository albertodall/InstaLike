using System;

namespace InstaLike.Core.Domain
{
    public class Follow : EntityBase<int>
    {
#pragma warning disable CS8618
        protected Follow() { }
#pragma warning restore CS8618

        public Follow(User follower, User followed) 
            : this()
        {
            Follower = follower;
            Followed = followed;
            FollowDate = DateTimeOffset.Now;
        }

        public virtual User Follower { get; }
        public virtual User Followed { get; }
        public virtual DateTimeOffset FollowDate { get; }
    }
}
